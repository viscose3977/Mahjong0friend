//YakuCalculate.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace MahjongGame
{
    public partial class MahjongGame
    {
        private int CalculateHan()
        {
            try
            {
                // 加入除錯輸出
                Console.WriteLine($"Debug: isRichi = {isRichi}");
                Console.WriteLine($"Debug: playerHand = {string.Join(", ", playerHand)}");
                int han = 0;

                // 一飜
                if (isRichi) han += 1; // 立直
                if (IsPinfu()) han += 1; // 平和
                if (IsTanyao()) han += 1; // 斷么九
                if (IsIipeikou()) han += 1; // 一盃口

                // 二飜
                if (isDoubleRiichi) han += 2; // 雙立直
                //缺三色同順
                if (IsIttsu()) han += 2; // 一氣通貫
                if (IsChanta()) han += 2; // 混全帶么九
                if (IsChitoitsu()) han += 2; // 七對子
                if (IsToitoi()) han += 2; // 對對和
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

        private int CalculateFu()
        {
            // 特殊情況
            if (IsChitoitsu()) return 25; // 七對子固定25符
            if (IsPinfu()) return 30; // 平和榮和30符

            int fu = 20; // 基本符數

            // 榮和不加自摸符

            // 門清加符（榮和時）
            if (!discardedTiles.Any()) fu += 10; // 門清

            // 計算面子和雀頭的符數
            var tiles = new List<string>(playerHand);

            // 找出雀頭（對子）
            var pairs = tiles
                .GroupBy(x => x.Replace("赤", ""))
                .Where(g => g.Count() == 2)
                .ToList();

            if (pairs.Any())
            {
                var pair = pairs.First();
                string pairTile = pair.Key;

                // 雀頭加符
                if (IsYakuhai(pairTile)) fu += 2; // 役牌雀頭
            }

            // 找出刻子和槓子
            var kotsuList = GetKotsuList();
            foreach (var kotsuTile in kotsuList)
            {
                bool isYaochu = ContainsYaochu(kotsuTile);
                bool isConcealed = !discardedTiles.Contains(kotsuTile);
                int count = playerHand.Count(t => t.Replace("赤", "") == kotsuTile);

                if (count == 4) // 槓子
                {
                    fu += isConcealed ? 32 : 16; // 暗槓/明槓
                }
                else // 刻子
                {
                    fu += isConcealed ? 8 : 2; // 暗刻/明刻（榮和時明刻只有2符）
                }

                // 么九牌加符
                if (isYaochu)
                {
                    if (count == 4) fu += 16; // 槓子的么九牌額外符數
                    else fu += 4; // 刻子的么九牌額外符數
                }
            }

            // 符數進位到10
            fu = ((fu + 9) / 10) * 10;

            // 最低30符
            return Math.Max(30, fu);
        }
        private int CalculateDoraCount()
        {
            // 加入除錯輸出
            Console.WriteLine($"Debug: doraIndicators = {string.Join(", ", doraIndicators)}");

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
            Console.WriteLine($"Debug: uraDoraIndicators = {string.Join(", ", uraDoraIndicators)}");

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
                Console.WriteLine($"GetDoraFromIndicator 發生錯誤：{ex.Message}");
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
                return 48000 * yakumanMultiplier; // 莊家役滿 * 役滿倍數
            }

            // 累計役滿
            if (han >= 13)
            {
                return 48000; // 莊家役滿
            }

            // 計算基本點數
            int basePoints = fu * (int)Math.Pow(2, han + 2);

            // 根據不同的點數上限進行調整
            if (basePoints >= 2000) // 滿貫
            {
                if (han >= 11) return 36000; // 莊家三倍滿
                if (han >= 8) return 24000;  // 莊家倍滿
                if (han >= 6) return 18000;  // 莊家跳滿
                return 12000; // 莊家滿貫
            }

            // 其他情況下，計算符數和飜數的點數
            return basePoints * 6; // 莊家點數
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
            if (IsRyuuiisou())  // 新增綠一色判定
            {
                multiplier += 1;
                yakumanList.Add("綠一色");
            }

            this.yakumanYakuList = yakumanList;
            return Math.Min(6, multiplier);
        }

        private void DisplayYakumanStatus(int han) //我忘了為甚麼寫 反正先不刪掉
        {
            int yakumanMultiplier = CalculateYakumanMultiplier();
            if (yakumanMultiplier > 0)
            {
                if (yakumanMultiplier == 1)
                    Console.WriteLine("役滿！");
                else if (yakumanMultiplier == 2)
                    Console.WriteLine("兩倍役滿！");
                else
                    Console.WriteLine($"{yakumanMultiplier}倍役滿！");
            }
            else if (han >= 13)
            {
                Console.WriteLine("累積役滿！");
            }
        }
    }
}