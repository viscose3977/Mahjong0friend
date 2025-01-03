using System;
using System.Collections.Generic;
using System.Linq;

namespace MahjongGame
{
    public partial class MahjongGame
    {
        private bool IsTenpai()
        {
            // 檢查當前手牌是否已經可以和牌
            if (IsBasicWinningHand(playerHand) && HasYaku(playerHand))
            {
                Console.WriteLine("已組成牌型，可進行和牌動作。");
                return true;
            }

            // 檢查每張可能的打牌，是否能進入聽牌狀態
            foreach (string discardTile in playerHand)
            {
                var testHand = new List<string>(playerHand);
                testHand.Remove(discardTile);

                // 判斷打掉一張牌後的可能待牌
                var waitingTiles = GetPossibleWaitingTiles(testHand);

                if (waitingTiles.Any())
                {
                    // 確認待牌是否有役，並決定訊息顯示
                    bool hasYaku = false;
                    foreach (var waitTile in waitingTiles)
                    {
                        var winningHand = new List<string>(testHand);
                        winningHand.Add(waitTile);

                        // 如果可以和牌並有役
                        if (IsBasicWinningHand(winningHand) && HasYaku(winningHand))
                        {
                            hasYaku = true;
                            break;
                        }
                    }

                    // 顯示進張訊息或提示無役
                    if (!hasYaku)
                    {
                        Console.WriteLine("無役，請進行立直。");
                    }
                    else
                    {
                        Console.WriteLine($"打{discardTile}聽{string.Join("、", waitingTiles)}");
                    }

                    // 如果是進張狀態，不返回和牌的結果
                    return true;
                }
            }

            return false;
        }

        // 獲取可能的待牌
        private List<string> GetPossibleWaitingTiles(List<string> hand)
        {
            var waitingTiles = new HashSet<string>();
            var handBySuit = hand
                .GroupBy(t => GetSuit(t.Replace("赤", "")))
                .ToDictionary(g => g.Key, g => g.ToList());

            // 檢查每個花色的牌
            foreach (var suitGroup in handBySuit)
            {
                if (string.IsNullOrEmpty(suitGroup.Key)) continue;

                var tiles = suitGroup.Value;
                // 檢查所有可能的順子組合
                for (int i = 1; i <= 9; i++)
                {
                    if (tiles.Contains(GetTileString(i, suitGroup.Key)) &&
                        tiles.Contains(GetTileString(i + 1, suitGroup.Key)))
                    {
                        // 順子兩面待ち：如三餅、六餅
                        if (i > 1) waitingTiles.Add(GetTileString(i - 1, suitGroup.Key)); // 前方
                        if (i + 2 <= 9) waitingTiles.Add(GetTileString(i + 2, suitGroup.Key)); // 後方
                    }

                    if (tiles.Contains(GetTileString(i, suitGroup.Key)) &&
                        tiles.Contains(GetTileString(i + 2, suitGroup.Key)))
                    {
                        // 順子嵌張待ち：如五餅
                        waitingTiles.Add(GetTileString(i + 1, suitGroup.Key));
                    }

                    if (tiles.Contains(GetTileString(i + 1, suitGroup.Key)) &&
                        tiles.Contains(GetTileString(i + 2, suitGroup.Key)))
                    {
                        // 順子前張待ち：如二餅
                        if (i > 0) waitingTiles.Add(GetTileString(i, suitGroup.Key));
                    }
                }
            }

            // 檢查對子待ち
            var allTiles = hand.Select(t => t.Replace("赤", ""));
            var pairs = allTiles.GroupBy(t => t)
                               .Where(g => g.Count() == 1);

            foreach (var pair in pairs)
            {
                string waitTile = pair.Key;
                var testHand = new List<string>(hand);
                testHand.Add(waitTile);
                if (IsBasicWinningHand(testHand))
                {
                    waitingTiles.Add(waitTile);
                }
            }

            // 最後檢查每個待牌是否真的可以和牌
            return waitingTiles.Where(tile => {
                var testHand = new List<string>(hand);
                testHand.Add(tile);
                return IsBasicWinningHand(testHand) && HasYaku(testHand);
            }).ToList();
        }


        private bool HasYaku(List<string> hand)
        {
            // 臨時保存原始手牌
            var originalHand = new List<string>(playerHand);

            // 暫時替換手牌進行檢查
            playerHand = new List<string>(hand);

            // 獲取役種列表（不含寶牌）
            var yakuList = GetYakuListWithoutDora();

            // 恢復原始手牌
            playerHand = originalHand;

            // 檢查是否有任何役種
            return yakuList.Any();
        }


        private static string GetTileString(int number, string suit)
        {
            string[] numbers = { "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            if (number < 1 || number > 9) return "";
            return numbers[number - 1] + suit;
        }
        private int CalculateHan()
        {
            try
            {
                // 加入除錯輸出
                //Console.WriteLine($"Debug: isRichi = {isRichi}");
                //Console.WriteLine($"Debug: playerHand = {string.Join(", ", playerHand)}");
                int han = 0;

                // 一飜
                if (isRichi) han += 1; // 立直
                if (IsPinfu()) han += 1; // 平和
                if (IsTanyao()) han += 1; // 斷么九
                if (IsIipeikou()) han += 1; // 一盃口

                // 二飜
                if (isDoubleRiichi) han += 2; // 兩立直
                //缺三色同順
                if (IsIttsu()) han += 2; // 一氣通貫
                if (IsChanta()) han += 2; // 混全帶么九
                if (IsChitoitsu()) han += 2; // 七對子
                //if (IsToitoi()) han += 2; // 對對和
                if (IsSanankou()) han += 2; // 三暗刻
                if (IsSanshokuDoukou()) han += 2; // 三色同刻
                if (IsHonroutou()) han += 2; // 混老頭
                if (IsShouSangen()) han += 2; // 小三元

                // 三飜
                if (IsHonitsu()) han += 3; // 混一色
                if (IsJunchan()) han += 3; // 純全帶么九
                if (IsRyanpeikou()) han += 3; // 二盃口

                // 六飜
                if (IsChinitsu()) han += 6; // 清一色

                // 寶牌計算
                han += CalculateDoraCount();
                han += playerHand.Count(tile => tile.Contains("赤")); // 赤寶牌

                // 立直時計算裏寶牌
                if (isRichi)
                {
                    han += CalculateUraDoraCount();
                }

                return han;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"計算番數時發生錯誤：{ex.Message}");
                Console.WriteLine($"堆疊追蹤：{ex.StackTrace}");
                return 0;
            }
        }

        private List<List<string>> GetMentsuList()
        {
            var mentsuList = new List<List<string>>();
            var tiles = new List<string>(playerHand);

            // 1. 從已槓牌資訊中獲取槓子
            foreach (var kangTile in kangTiles)
            {
                var kang = new List<string> { kangTile, kangTile, kangTile, kangTile };
                mentsuList.Add(kang);
            }

            // 2. 找出刻子（手牌中被括號標記的部分）
            var kotsuGroups = tiles
                .Where(t => t.StartsWith("("))
                .GroupBy(t => t.Replace("赤", "").Trim('(', ')'))
                .Where(g => g.Count() == 3);

            foreach (var group in kotsuGroups)
            {
                mentsuList.Add(group.ToList());
            }

            // 3. 找出順子（剩餘的牌中尋找）
            var remainingTiles = tiles.Where(t => !t.StartsWith("(")).ToList();
            while (remainingTiles.Any())
            {
                var tile = remainingTiles.First();
                if (!IsJihai(tile))  // 使用現有的 IsJihai 方法
                {
                    var (num, suit) = GetNumberAndType(tile);
                    if (num <= 7)
                    {
                        string tile2 = GetTileString(num + 1, suit);
                        string tile3 = GetTileString(num + 2, suit);
                        if (remainingTiles.Contains(tile2) && remainingTiles.Contains(tile3))
                        {
                            var shuntsu = new List<string> { tile, tile2, tile3 };
                            mentsuList.Add(shuntsu);
                            remainingTiles.Remove(tile);
                            remainingTiles.Remove(tile2);
                            remainingTiles.Remove(tile3);
                            continue;
                        }
                    }
                }
                remainingTiles.Remove(tile);
            }

            return mentsuList;
        }
        private int CalculateFu()
        {
            // 特殊情況
            if (IsChitoitsu()) return 25;  // 七對子固定25符
            if (IsPinfu()) return 30;      // 平和榮和30符

            int fu = 20;  // 基本符數
            fu += 10;     // 門清榮和必定加10符

            // 計算面子和雀頭的符數
            var tiles = new List<string>(playerHand);

            // 計算雀頭符數
            var pairs = tiles
                .GroupBy(x => x.Replace("赤", ""))
                .Where(g => g.Count() == 2)
                .ToList();

            if (pairs.Any())
            {
                var pairTile = pairs.First().Key;
                if (IsValueTile(pairTile))  // 自風、場風、三元牌
                {
                    fu += 2;
                }
            }

            // 計算面子符數
            var mentsuList = GetMentsuList();
            foreach (var mentsu in mentsuList)
            {
                if (IsKotsu(mentsu))  // 刻子
                {
                    bool isYaochu = IsYaochuOrHonor(mentsu[0]);

                    if (mentsu.Count == 4)  // 槓子
                    {
                        // 單人麻將全算暗槓
                        fu += isYaochu ? 32 : 16;
                    }
                    else  // 刻子
                    {
                        // 單人麻將全算暗刻
                        fu += isYaochu ? 8 : 4;
                    }
                }
                // 順子不加符
            }

            // 符數進位到10
            fu = ((fu + 9) / 10) * 10;

            // 最低30符
            return Math.Max(30, fu);
        }

        // 判斷是否為役牌（自風、場風、三元牌）
        private bool IsValueTile(string tile)
        {
            // 三元牌
            if (tile == "白" || tile == "發" || tile == "中") return true;

            // 自風牌
            if (tile == playerWind) return true;

            // 場風牌
            if (tile == roundWind) return true;

            return false;
        }

        // 判斷是否為么九牌或字牌
        private bool IsYaochuOrHonor(string tile)
        {
            // 字牌
            if (tile == "東" || tile == "南" || tile == "西" || tile == "北" ||
                tile == "白" || tile == "發" || tile == "中") return true;

            // 么九牌
            if (tile.StartsWith("一") || tile.StartsWith("九")) return true;

            return false;
        }

        // 判斷是否為刻子
        private bool IsKotsu(List<string> tiles)
        {
            if (tiles.Count < 3) return false;
            var firstTile = tiles[0].Replace("赤", "");
            return tiles.All(t => t.Replace("赤", "") == firstTile);
        }
        private int CalculateDoraCount()
        {
            // 加入除錯輸出
            //Console.WriteLine($"Debug: doraIndicators = {string.Join(", ", doraIndicators)}");

            int count = 0;
            foreach (var indicator in doraIndicators.Take(kangCount + 1))
            {
                string actualDora = GetDoraFromIndicator(indicator);
                count += playerHand.Count(t => t.Replace("赤", "") == actualDora);
            }
            return count;
        }

        private int CalculateUraDoraCount()
        {
            // 加入除錯輸出
            //Console.WriteLine($"Debug: uraDoraIndicators = {string.Join(", ", uraDoraIndicators)}");

            int count = 0;
            foreach (var indicator in uraDoraIndicators.Take(kangCount + 1))
            {
                string actualDora = GetDoraFromIndicator(indicator);
                count += playerHand.Count(t => t.Replace("赤", "") == actualDora);
            }
            return count;
        }


        private string GetDoraFromIndicator(string indicator)
        {
            try
            {
                if (indicator.StartsWith("赤"))
                {
                    indicator = indicator.Replace("赤", ""); // 移除"赤"字
                }
                // 處理數牌
                if (indicator.EndsWith("萬") || indicator.EndsWith("餅") || indicator.EndsWith("索"))
                {
                    string[] numbers = { "一", "二", "三", "四", "五", "六", "七", "八", "九" };
                    string number = indicator.Substring(0, 1); // 提取漢字數字
                    string suit = indicator.Substring(1);      // 提取花色（萬、餅、索）

                    int index = Array.IndexOf(numbers, number);
                    if (index == -1)
                        throw new Exception($"無法解析的寶牌指示：{indicator}");

                    // 計算下一張牌（環繞到一）
                    string nextNumber = numbers[(index + 1) % 9];
                    return nextNumber + suit;
                }

                // 處理字牌
                switch (indicator)
                {
                    case "東": return "南";
                    case "南": return "西";
                    case "西": return "北";
                    case "北": return "東";
                    case "白": return "發";
                    case "發": return "中";
                    case "中": return "白";
                    default:
                        throw new Exception($"無法解析的寶牌指示：{indicator}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"debug:GetDoraFromIndicator 發生錯誤：{ex.Message}");
                return "";
            }
        }



        private string GetNextTile(string tile)
        {
            if (tile.EndsWith("萬") || tile.EndsWith("餅") || tile.EndsWith("索"))
            {
                string suit = tile.Substring(tile.Length - 1);
                string number = tile.Substring(0, tile.Length - 1);
                string[] numbers = { "一", "二", "三", "四", "五", "六", "七", "八", "九" };
                int index = Array.IndexOf(numbers, number);
                if (index == -1) return tile;
                return numbers[(index + 1) % 9] + suit;
            }
            else
            {
                string[] winds = { "東", "南", "西", "北" };
                string[] dragons = { "白", "發", "中" };

                int windIndex = Array.IndexOf(winds, tile);
                if (windIndex != -1)
                {
                    return winds[(windIndex + 1) % 4];
                }

                int dragonIndex = Array.IndexOf(dragons, tile);
                if (dragonIndex != -1)
                {
                    return dragons[(dragonIndex + 1) % 3];
                }
            }
            return tile;
        }

        private int CalculateBasePoints(int han, int fu)
        {
            // 役滿
            int yakumanMultiplier = CalculateYakumanMultiplier();
            if (yakumanMultiplier > 0)
            {
                return 48000 * yakumanMultiplier;
            }

            // 累計役滿
            if (han >= 13)
            {
                return 48000;
            }

            // 特殊點數
            if (han >= 11) return 36000;     // 三倍滿
            if (han >= 8) return 24000;      // 倍滿
            if (han >= 6) return 18000;      // 跳滿
            if (han >= 5 || (han >= 4 && fu >= 40) || (han >= 3 && fu >= 70)) return 12000;      // 滿貫

            Dictionary<(int han, int fu), int> pointTable = new Dictionary<(int han, int fu), int>
    {
        // 一番
        {(1, 30), 1500}, {(1, 40), 2000}, {(1, 50), 2400},
        {(1, 60), 2900}, {(1, 70), 3400}, {(1, 80), 3900},
        {(1, 90), 4400}, {(1, 100), 4800}, {(1, 110), 5300},
        
        // 二番
        {(2, 30), 2400}, {(2, 40), 2900}, {(2, 50), 3900},
        {(2, 60), 4800}, {(2, 70), 5800}, {(2, 80), 6800},
        {(2, 90), 7700}, {(2, 100), 8700}, {(2, 110), 9600},
        
        // 三番
        {(3, 30), 4800}, {(3, 40), 5800}, {(3, 50), 7700},
        {(3, 60), 9600}, {(3, 70), 11600},
        
        // 四番
        {(4, 30), 9600}, {(4, 40), 11600}
    };

            // 查表獲取點數
            if (pointTable.TryGetValue((han, fu), out int points))
            {
                return points;
            }

            // 如果找不到對應的點數，返回0並輸出錯誤訊息
            Console.WriteLine($"警告：無法找到 {han}番{fu}符 的對應點數");
            return 0;
        }

        private int CalculateYakumanMultiplier()
        {
            int multiplier = 0;
            List<string> yakumanList = new List<string>();

            // 檢查役滿役種並記錄
            if (IsKokushi())
            {
                if (IsKokushimusouJuusanmenmachi())
                {
                    multiplier += 2;
                    yakumanList.Add("國士無雙十三面");
                }
                else
                {
                    multiplier += 1;
                    yakumanList.Add("國士無雙");
                }
            }

            if (IsSuuankou())
            {
                if (IsSuankoTanki())
                {
                    multiplier += 2;
                    yakumanList.Add("四暗刻單騎");
                }
                else
                {
                    multiplier += 1;
                    yakumanList.Add("四暗刻");
                }
            }

            if (IsChuurenpoutou())
            {
                if (IsJunseiChuurenpoutou())
                {
                    multiplier += 2;
                    yakumanList.Add("純正九蓮寶燈");
                }
                else
                {
                    multiplier += 1;
                    yakumanList.Add("九蓮寶燈");
                }
            }

            // 檢查其他役滿
            if (IsDaisangen())
            {
                multiplier += 1;
                yakumanList.Add("大三元");
            }
            if (IsShousuushi())
            {
                multiplier += 1;
                yakumanList.Add("小四喜");
            }
            if (IsDaisuushi())
            {
                multiplier += 2;
                yakumanList.Add("大四喜");
            }
            if (IsTsuuiisou())
            {
                multiplier += 1;
                yakumanList.Add("字一色");
            }
            if (IsChinroutou())
            {
                multiplier += 1;
                yakumanList.Add("清老頭");
            }
            if (IsSuukantsu())
            {
                multiplier += 1;
                yakumanList.Add("四槓子");
            }
            if (IsTenhou())
            {
                multiplier += 1;
                yakumanList.Add("天和");
            }
            if (IsRyuuiisou())
            {
                multiplier += 1;
                yakumanList.Add("綠一色");
            }

            this.yakumanYakuList = yakumanList;
            return Math.Min(6, multiplier);
        }
    }
}