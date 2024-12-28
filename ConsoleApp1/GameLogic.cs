using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MahjongGame
{
    public partial class MahjongGame
    {
        private void DiscardTile()
        {
            Console.WriteLine("請選擇要打出的牌 (1-14): ");
            if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > playerHand.Count)
            {
                Console.WriteLine("無效的選擇！");
                return;
            }

            // 計算槓牌組的總數
            var markedGroups = playerHand
                .Where(t => t.StartsWith("*"))
                .GroupBy(t => t.Replace("*", "").Replace("赤", ""))
                .ToList();

            int totalMarkedTiles = markedGroups.Sum(g => g.Count());

            // 如果選擇的編號在槓牌組範圍內
            if (index <= totalMarkedTiles)
            {
                Console.WriteLine("該牌為槓牌組的一部分，不可打出！");
                Console.WriteLine("按任意鍵繼續...");
                Console.ReadKey();
                return;
            }

            // 獲取非槓牌的列表
            var normalTiles = playerHand.Where(t => !t.StartsWith("*")).ToList();

            // 計算在非槓牌中的實際索引
            int normalIndex = index - totalMarkedTiles - 1;

            if (normalIndex >= 0 && normalIndex < normalTiles.Count)
            {
                string selectedTile = normalTiles[normalIndex];

                // 從手牌中移除選中的牌
                playerHand.Remove(selectedTile);
                discardedTiles.Add(selectedTile);

                // 檢查四風連打
                if (isFirstRound && (selectedTile == "東" || selectedTile == "南" ||
                                    selectedTile == "西" || selectedTile == "北"))
                {
                    firstRoundWindTiles.Add(selectedTile);
                    if (firstRoundWindTiles.Count == 4 && firstRoundWindTiles.Distinct().Count() == 1)
                    {
                        Console.WriteLine("四風連打！流局！");
                        deck.Clear();
                        return;
                    }
                }

                // 重置和牌後的限制狀態
                hasWon = false;  // 解除和牌和立直限制

                DrawTile();
            }
            else
            {
                Console.WriteLine("無效的選擇！");
                return;
            }
        }
        private void DrawTile()
        {
            if (deck.Count > 0)
            {
                string drawnTile = deck[0];
                deck.RemoveAt(0);

                // 先排序前13張牌
                if (playerHand.Count >= 13)
                {
                    var initialHand = playerHand.Take(13).ToList();
                    initialHand.Sort((a, b) => GetTileSortOrder(a.Replace("赤", ""))
                                            .CompareTo(GetTileSortOrder(b.Replace("赤", ""))));
                    playerHand = initialHand;
                }

                // 將摸到的牌放在第14位置
                playerHand.Add(drawnTile);

                Console.WriteLine($"摸到了: {drawnTile}");
                Console.WriteLine("按任意鍵繼續...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("荒牌流局！");
                if (winningRecord.Any())
                {
                    Console.WriteLine("和牌紀錄: " + string.Join(", ", winningRecord));
                }
                if (!CheckLevelComplete(0, 0))
                {
                    Console.WriteLine("未達成通關條件，遊戲結束！");
                }
            }
        }

        private void DeclareRiichi()
        {
            // 1. 基本檢查
            if (hasWon)
            {
                Console.WriteLine("該牌型已和過牌，無法再次立直。");
                Console.WriteLine("請打出至少一張牌後再進行立直。");
                Console.WriteLine("按任意鍵繼續...");
                Console.ReadKey();
                return;
            }

            if (isRichi || isDoubleRiichi)
            {
                Console.WriteLine("已經立直！");
                Console.WriteLine("按任意鍵繼續...");
                Console.ReadKey();
                return;
            }

            // 2. 檢查點數是否足夠
            if (playerPoints < 1000)
            {
                Console.WriteLine("點數不足，無法立直！");
                Console.WriteLine("按任意鍵繼續...");
                Console.ReadKey();
                return;
            }

            // 3. 確認立直意願
            Console.WriteLine("確定要立直嗎？(Y/N)");
            if (Console.ReadLine().ToUpper() != "Y")
            {
                Console.WriteLine("取消立直。");
                return;
            }

            // 4. 檢查是否真的聽牌
            if (!IsTenpai())
            {
                Console.WriteLine("\n=== 假立直被抓到了！===");
                Console.WriteLine("未聽牌，判定為假立直！");
                Console.WriteLine("請仔細審視手牌，進行罰符4000點！");
                DeductPoints(4000);
                Console.WriteLine($"當前點數：{playerPoints}");
                Console.WriteLine("按任意鍵繼續...");
                Console.ReadKey();
                return;
            }

            // 5. 執行立直
            playerPoints -= 1000; // 立直棒
            richiCount++;
            isRichi = true;
            // 修改這裡：只有在第一巡且沒有任何棄牌時才能兩立直
            isDoubleRiichi = isFirstRound && discardedTiles.Count == 0;

            // 6. 顯示結果
            if (isDoubleRiichi)
                Console.WriteLine("兩立直成功！");
            else
                Console.WriteLine("立直成功！");
            Console.WriteLine($"當前點數：{playerPoints}");
            Console.WriteLine("按任意鍵繼續...");
            Console.ReadKey();
        }
        private void DeductPoints(int points)
        {
            playerPoints -= points;
            if (playerPoints < 0)
            {
                Console.Clear();
                Console.WriteLine("\n偵測到玩家已達到負分，獲得成就：【麻將下手】");
                Console.WriteLine("遊戲失敗，即將返回主畫面...");
                Console.ReadKey();
                hasWon = true;  // 設置遊戲結束標誌
                deck.Clear();   // 清空牌山以結束遊戲
                Environment.Exit(0);  // 直接結束程序
            }
        }
        private void DeclareKang()
        {
            // 將手牌按類型分組，排除已經槓過的牌
            var kangGroups = playerHand
                .GroupBy(x => x.Replace("赤", ""))
                .Where(g => g.Count() == 4 && !kangTiles.Contains(g.Key))
                .ToList();

            if (!kangGroups.Any())
            {
                Console.WriteLine("\n=== 詐槓被抓到了！===");
                Console.WriteLine("詐槓！請仔細審視手牌，進行罰符4000點！");
                DeductPoints(4000);
                Console.WriteLine($"當前點數：{playerPoints}");
                Console.WriteLine("按任意鍵繼續...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("可以槓的牌：");
            for (int i = 0; i < kangGroups.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {kangGroups[i].Key}");
            }

            Console.WriteLine("請選擇要槓的牌（輸入數字），或按Enter取消：");
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int choice) ||
                choice < 1 || choice > kangGroups.Count)
            {
                return;
            }

            string selectedTile = kangGroups[choice - 1].Key;
            kangTiles.Add(selectedTile);

            // 重新組織手牌
            var tilesToKang = playerHand
                .Where(tile => tile.Replace("赤", "") == selectedTile)
                .ToList();

            var otherTiles = playerHand
                .Where(tile => tile.Replace("赤", "") != selectedTile)
                .ToList();

            // 清空當前手牌
            playerHand.Clear();

            // 先加入三張標記過的牌
            playerHand.AddRange(tilesToKang.Take(3).Select(tile => $"*{tile}"));

            // 再加入其他牌
            playerHand.AddRange(otherTiles);

            kangCount++;
            Console.WriteLine($"槓！{selectedTile}");

            // 補牌
            if (deck.Count > 0)
            {
                DrawTile();
            }
        }

        private void InvalidWinAttempt()
        {
            Console.WriteLine("\n=== 詐和被抓到了！===");
            Console.WriteLine("無役詐和！請仔細審視手牌，進行罰符罰分4000點！");
            DeductPoints(4000);
            Console.WriteLine($"當前點數：{playerPoints}");
            Console.WriteLine("按任意鍵繼續...");
            Console.ReadKey();
        }

        private bool CanDeclareKang()
        {
            // 判定玩家手牌是否有 4 張相同的牌且符合條件
            var kangGroups = playerHand
                .GroupBy(x => x.Replace("赤", ""))
                .Where(g => g.Count() == 4 && !kangTiles.Contains(g.Key))
                .ToList();

            // 如果有可槓的牌，返回 true；否則返回 false
            return kangGroups.Any();
        }




        private bool CheckLevelComplete(int han, int fu)
        {
            // 如果是役滿，直接通關
            if (han >= 13 || yakumanYakuList.Any())
            {
                return true;
            }

            // 一般和牌的通關條件判斷
            switch (level)
            {
                case 1: // 三番30符起
                    return han >= 3 && fu >= 30;
                case 2: // 滿貫（4番40符起）
                    return (han >= 4 && fu >= 40) || (han >= 5);
                case 3: // 跳滿（六番起）
                    return han >= 6;
                case 4: // 倍滿（八番起）
                    return han >= 8;
                case 5: // 三倍滿（十一番起）
                    return han >= 11;
                case 6: // 役滿（十三番起）
                    return han >= 13;
                default:
                    return false;
            }
        }


        private void CompleteHand()
        {
            if (hasWon)
            {
                Console.WriteLine("該牌型已和過牌，無法再次和牌。");
                Console.WriteLine("請打出至少一張牌後，再進行和牌。");
                Console.WriteLine("按任意鍵繼續...");
                Console.ReadKey();
                return;
            }

            // 1. 先檢查基本和牌型
            if (!IsBasicWinningHand(playerHand))
            {
                // 如果不是基本和牌型，檢查是否為特殊和牌型（七對子等）
                // TODO: 添加特殊和牌型檢查
                // 如果都不是，則和牌失敗
                InvalidWinAttempt();
                return;
            }

            // 2. 取得役種列表
            List<(string name, int value)> yakuList = GetYakuList();

            // 3. 檢查是否有役種（包含立直）
            if (!yakuList.Any())
            {
                InvalidWinAttempt();
                return;
            }

            // 4. 確認和牌
            hasWon = true;


            // 5. 計算分數
            int yakumanMultiplier = CalculateYakumanMultiplier();
            int basePoints = yakumanMultiplier > 0 ?
                48000 * yakumanMultiplier :
                CalculateBasePoints(yakuList.Sum(y => y.value), CalculateFu());

            // 6. 計算立直點數
            int points = basePoints;
            int richiPoints = 0;
            if (isRichi)
            {
                richiPoints = 1000 * richiCount;
                points += richiPoints;
            }
            // 7. 顯示和牌結果
            if (yakumanMultiplier > 0)
            {
                // 役滿的情況
                string multiplierText = yakumanMultiplier > 1 ? $"{yakumanMultiplier}倍" : "";
                Console.WriteLine($"\n和牌！ - {multiplierText}役滿");

                // 使用 yakumanYakuList 顯示役滿名稱
                foreach (var yakuName in yakumanYakuList)
                {
                    Console.WriteLine(yakuName);
                }

                Console.WriteLine($"\n獲得 {basePoints}點");
            }
            else
            {
                // 一般和牌的情況
                Console.WriteLine("\n和牌！ - ");
                foreach (var yaku in yakuList)
                {
                    Console.WriteLine($"{yaku.name} {yaku.value}番");
                }

                Console.Write("\n總計 ");
                string rankName = GetHandRank(yakuList.Sum(y => y.value), CalculateFu());
                if (!string.IsNullOrEmpty(rankName))
                {
                    Console.Write($"{rankName} ");
                }
                Console.WriteLine($"{yakuList.Sum(y => y.value)}番{CalculateFu()}符");
            }
            // 8.顯示點數資訊
            Console.WriteLine($"\n獲得 {points}點");
            if (richiPoints > 0)
            {
                Console.WriteLine($"歸還立直點 {richiPoints}點");
            }
            Console.WriteLine($"目前共有 {playerPoints}點\n");

            // 9. 記錄和牌資訊
            string record = "和牌 - ";
            if (yakumanMultiplier > 0)
            {
                string multiplierText = yakumanMultiplier > 1 ? $"{yakumanMultiplier}倍" : "";
                record += $"{multiplierText}役滿 {string.Join(" ", yakumanYakuList)}";
            }
            else
            {
                string rankName = GetHandRank(yakuList.Sum(y => y.value), CalculateFu());
                if (!string.IsNullOrEmpty(rankName))
                {
                    record += $"{rankName} ";
                }
                record += $"{yakuList.Sum(y => y.value)}番{CalculateFu()}符";
            }
            record += $", 獲得：{points}點";
            winningRecord.Add(record);

            // 10.顯示關卡資訊
            Console.WriteLine($"本關{GetLevelObjective()}");
            Console.WriteLine("本關和牌紀錄：");
            if (winningRecord.Any())
            {
                foreach (var previousRecord in winningRecord)
                {
                    Console.WriteLine(previousRecord);
                }
            }
            else
            {
                Console.WriteLine("無");
            }


            // 11. 檢查關卡完成
            if (!levelCompleted)
            {
                levelCompleted = CheckLevelComplete(yakuList.Sum(y => y.value), CalculateFu());
            }

            // 如果還是沒達成通關條件
            if (!levelCompleted)
            {
                Console.WriteLine("\n未達成通關條件，遊戲繼續。");
                isRichi = false;
                isDoubleRiichi = false;
                hasWon = true;
                richiCount = 0;
                ResetDoraIndicators();  // 重置寶牌指示牌順序
                Console.WriteLine("\n按任意鍵繼續...");
                Console.ReadKey();
                return;
            }

            // 達成通關條件後的處理
            if (level == 6)  // 在第六關
            {
                Console.WriteLine("\n已達成通關條件，是否繼續留在本關卡？(Y/N)");
                string choice = Console.ReadLine().ToUpper();

                if (choice == "Y")
                {
                    isRichi = false;
                    isDoubleRiichi = false;
                    hasWon = true;
                    richiCount = 0;
                    ResetDoraIndicators();
                    return;
                }
                else
                {
                    ShowGameClearScreen();  // 顯示通關畫面
                }
            }
            else  // 其他關卡
            {
                Console.WriteLine("\n已達成通關條件，是否繼續留在本關卡？(Y/N)");
                string choice = Console.ReadLine().ToUpper();

                if (choice == "Y")
                {
                    isRichi = false;
                    isDoubleRiichi = false;
                    hasWon = true;
                    richiCount = 0;
                    ResetDoraIndicators();
                    return;
                }

                Console.WriteLine("\n是否進入下一關卡？(Y/N)");
                choice = Console.ReadLine().ToUpper();

                if (choice == "Y")
                {
                    level++; // 進入下一關
                    ResetGame();
                }
                else
                {
                    Console.WriteLine("\n感謝遊玩，即將退出遊戲...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
        }
        private void ResetDoraIndicators()
        {
            Console.WriteLine($"Debug: 重置前的寶牌 = {string.Join(", ", doraIndicators)}");

            // 保存當前寶牌的副本
            var currentDoras = new List<string>(doraIndicators);
            var currentUraDoras = new List<string>(uraDoraIndicators);

            // 打亂當前明寶的順序
            Random rnd = new Random();
            int n = currentDoras.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                string temp = currentDoras[k];
                currentDoras[k] = currentDoras[n];
                currentDoras[n] = temp;
            }

            // 打亂當前裏寶的順序
            n = currentUraDoras.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                string temp = currentUraDoras[k];
                currentUraDoras[k] = currentUraDoras[n];
                currentUraDoras[n] = temp;
            }

            // 清空並重新設置寶牌
            doraIndicators.Clear();
            uraDoraIndicators.Clear();

            // 設置重新排序後的寶牌
            doraIndicators.AddRange(currentDoras);
            uraDoraIndicators.AddRange(currentUraDoras);

            Console.WriteLine($"Debug: 重置後的寶牌 = {string.Join(", ", doraIndicators)}");
        }
        private string GetHandRank(int han, int fu)
        {
            if (han >= 13) return "累積役滿";
            if (han >= 11) return "三倍滿";
            if (han >= 8) return "倍滿";
            if (han >= 6) return "跳滿";
            if (han >= 4 || (han == 3 && fu >= 70) || (han == 4 && fu >= 40)) return "滿貫";
            return "";
        }

        private List<(string name, int value)> GetYakuList(bool checkDora = true)
        {
            List<(string name, int value)> yakuList = new List<(string name, int value)>();
            bool hasYakuman = false;

            // 檢查所有役滿，不要提前返回
            // 兩倍役滿
            if (IsDaisuushi())
            {
                yakuList.Add(("大四喜", 26));
                hasYakuman = true;
            }
            if (IsKokushimusouJuusanmenmachi())
            {
                yakuList.Add(("國士無雙十三面聽", 26));
                hasYakuman = true;
            }
            if (IsJunseiChuurenpoutou())
            {
                yakuList.Add(("純正九蓮寶燈", 26));
                hasYakuman = true;
            }
            if (IsSuankoTanki())
            {
                yakuList.Add(("四暗刻單騎", 26));
                hasYakuman = true;
            }

            // 一倍役滿
            if (IsSuuankou() && !IsSuankoTanki())
            {
                yakuList.Add(("四暗刻", 13));
                hasYakuman = true;
            }
            if (IsDaisangen())
            {
                yakuList.Add(("大三元", 13));
                hasYakuman = true;
            }
            if (IsShousuushi() && !IsDaisuushi())
            {
                yakuList.Add(("小四喜", 13));
                hasYakuman = true;
            }
            if (IsTsuuiisou())
            {
                yakuList.Add(("字一色", 13));
                hasYakuman = true;
            }
            if (IsChinroutou())
            {
                yakuList.Add(("清老頭", 13));
                hasYakuman = true;
            }
            if (IsSuukantsu())
            {
                yakuList.Add(("四槓子", 13));
                hasYakuman = true;
            }
            if (IsChuurenpoutou() && !IsJunseiChuurenpoutou())
            {
                yakuList.Add(("九蓮寶燈", 13));
                hasYakuman = true;
            }
            if (IsRyuuiisou())
            {
                yakuList.Add(("綠一色", 13));
                hasYakuman = true;
            }
            if (IsKokushi() && !IsKokushimusouJuusanmenmachi())
            {
                yakuList.Add(("國士無雙", 13));
                hasYakuman = true;
            }
            if (IsTenhou())
            {
                yakuList.Add(("天和", 13));
                hasYakuman = true;
            }

            // 如果有役滿，就不檢查一般役種
            if (hasYakuman) return yakuList;

            // 一般役種
            // 一飜
            // 一飜
            if (isDoubleRiichi)  // 先判斷是否為雙立直
            {
                yakuList.Add(("兩立直", 2));
            }
            else if (isRichi)    // 如果不是雙立直，才判斷一般立直
            {
                yakuList.Add(("立直", 1));
            }
            if (IsPinfu()) yakuList.Add(("平和", 1));
            if (IsTanyao()) yakuList.Add(("斷么九", 1));
            if (IsIipeikou()) yakuList.Add(("一盃口", 1));

            // 二飜
            //缺三色同順
            if (IsIttsu()) yakuList.Add(("一氣通貫", 2));
            if (IsChanta()) yakuList.Add(("混全帶么九", 2));
            if (IsChitoitsu()) yakuList.Add(("七對子", 2));
            if (IsToitoi()) yakuList.Add(("對對和", 2));
            if (IsSanankou()) yakuList.Add(("三暗刻", 2));
            if (IsSanshokuDoukou()) yakuList.Add(("三色同刻", 2));
            if (IsHonroutou()) yakuList.Add(("混老頭", 2));
            if (IsShouSangen()) yakuList.Add(("小三元", 2));

            // 三飜
            if (IsHonitsu()) yakuList.Add(("混一色", 3));
            if (IsJunchan()) yakuList.Add(("純全帶么九", 3));
            if (IsRyanpeikou()) yakuList.Add(("二盃口", 3));

            // 六飜
            if (IsChinitsu()) yakuList.Add(("清一色", 6));

            // 計算寶牌
            if (checkDora)
            {
                // 1. 赤寶牌
                int redDora = playerHand.Count(t => t.Contains("赤"));
                if (redDora > 0) yakuList.Add(($"赤寶牌(*{redDora})", redDora));

                // 2. 一般寶牌
                int normalDora = CalculateDoraCount();
                if (normalDora > 0) yakuList.Add(($"明寶牌(*{normalDora})", normalDora));

                // 3. 裏寶牌（只有立直時才計算）
                if (isRichi)
                {
                    int uraDora = CalculateUraDoraCount();
                    if (uraDora > 0) yakuList.Add(($"裏寶牌(*{uraDora})", uraDora));
                }
            }

            return yakuList;
        }

        private void ShowGameClearScreen()
        {
            Console.Clear();  // 清空畫面
            Console.WriteLine();
            Console.WriteLine("   _____     _____     _____     _____  ");
            Console.WriteLine("  /_____/|  /_____/|  /_____/|  /_____/|");
            Console.WriteLine(" |     | | |     | | |     | | |     | |       |\\      _,,,---,,_");
            Console.WriteLine(" | 恭  | | | 喜  | | | 通  | | | 關  | | ZZZzz /,`.-'`'    -.  ;-;;,_");
            Console.WriteLine(" |     | | |     | | |     | | |     | |      |,4-  ) )-,_. ,\\ (  `'-'");
            Console.WriteLine(" |-----|/  |-----|/  |-----|/  |-----|/     '---''(_/--'  `-'\\_)");
            Console.WriteLine("                               _____     _____     _____     _____");
            Console.WriteLine("                              /_____/|  /_____/|  /_____/|  /_____/|");
            Console.WriteLine("                             |     | | |     | | |     | | |     | |");
            Console.WriteLine("                             | 孤  | | | 兒  | | | 日  | | | 麻  | |");
            Console.WriteLine("                             |     | | |     | | |     | | |     | |");
            Console.WriteLine("                             |-----|/  |-----|/  |-----|/  |-----|/");
            Console.WriteLine();
            Console.WriteLine("\n遊戲結束！");
            Console.WriteLine("恭喜通關，也感謝您的耐心遊玩！");
            Console.WriteLine();
            Console.WriteLine("按下任意鍵離開遊戲...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}