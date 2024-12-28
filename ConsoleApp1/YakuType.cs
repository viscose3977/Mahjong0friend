﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MahjongGame
{
    public partial class MahjongGame
    {
        // 基本和牌型判定（一般型：順子、刻子、雀頭組合）
        public static bool IsBasicWinningHand(List<string> hand)
        {
            var sortedHand = new List<string>(hand);
            sortedHand.Sort();

            for (int i = 0; i < sortedHand.Count - 1; i++)
            {
                if (sortedHand[i] == sortedHand[i + 1])
                {
                    var remainingTiles = new List<string>(sortedHand);
                    remainingTiles.RemoveAt(i + 1);
                    remainingTiles.RemoveAt(i);

                    if (CanFormMentsu(remainingTiles))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //private static bool CanFormMentsu(List<string> tiles)
        //{
        //    if (tiles.Count == 0) return true;

        //    tiles.Sort();

        //    // 1. 先嘗試順子
        //    for (int i = 0; i < tiles.Count - 2; i++)
        //    {
        //        if (!IsNumberTile(tiles[i])) continue;

        //        var (num1, suit1) = GetNumberAndType(tiles[i]);

        //        string tile2 = GetTileString(num1 + 1, suit1);
        //        string tile3 = GetTileString(num1 + 2, suit1);

        //        if (tiles.Contains(tile2) && tiles.Contains(tile3))
        //        {
        //            var remaining = new List<string>(tiles);
        //            remaining.Remove(tiles[i]);
        //            remaining.Remove(tile2);
        //            remaining.Remove(tile3);
        //            if (CanFormMentsu(remaining)) return true;
        //        }
        //    }

        //    // 2. 再嘗試刻子
        //    for (int i = 0; i < tiles.Count - 2; i++)
        //    {
        //        if (tiles[i] == tiles[i + 1] && tiles[i + 1] == tiles[i + 2])
        //        {
        //            var remaining = new List<string>(tiles);
        //            remaining.RemoveAt(i + 2);
        //            remaining.RemoveAt(i + 1);
        //            remaining.RemoveAt(i);
        //            if (CanFormMentsu(remaining)) return true;
        //        }
        //    }

        //    return false;
        //}
        private static bool CanFormMentsu(List<string> tiles, int recursionLevel = 0)
        {
            if (tiles.Count == 0) return true;

            tiles.Sort();

            int sequenceCount = 0;  // 用來計數順子的個數
            int tripletCount = 0;   // 用來計數刻子的個數
            string pair = "";       // 用來儲存雀頭（對子）

            // 輸出當前的遞迴層數和目前牌組
            Console.WriteLine(new string(' ', recursionLevel * 2) + $"目前牌組: {string.Join(", ", tiles)}");

            // 1. 先嘗試順子
            for (int i = 0; i < tiles.Count - 2; i++)
            {
                if (!IsNumberTile(tiles[i])) continue;

                var (num1, suit1) = GetNumberAndType(tiles[i]);

                string tile2 = GetTileString(num1 + 1, suit1);
                string tile3 = GetTileString(num1 + 2, suit1);

                if (tiles.Contains(tile2) && tiles.Contains(tile3))
                {
                    sequenceCount++;
                    Console.WriteLine(new string(' ', recursionLevel * 2) + $"找到第{sequenceCount}個順子: {tiles[i]}, {tile2}, {tile3}");

                    var remaining = new List<string>(tiles);
                    remaining.Remove(tiles[i]);
                    remaining.Remove(tile2);
                    remaining.Remove(tile3);
                    if (CanFormMentsu(remaining, recursionLevel + 1)) return true;
                }
            }

            // 2. 再嘗試刻子
            for (int i = 0; i < tiles.Count - 2; i++)
            {
                if (tiles[i] == tiles[i + 1] && tiles[i + 1] == tiles[i + 2])
                {
                    tripletCount++;
                    Console.WriteLine(new string(' ', recursionLevel * 2) + $"找到第{tripletCount}個刻子: {tiles[i]}");

                    var remaining = new List<string>(tiles);
                    remaining.RemoveAt(i + 2);
                    remaining.RemoveAt(i + 1);
                    remaining.RemoveAt(i);
                    if (CanFormMentsu(remaining, recursionLevel + 1)) return true;
                }
            }

            // 3. 嘗試尋找雀頭（對子）
            for (int i = 0; i < tiles.Count - 1; i++)
            {
                if (tiles[i] == tiles[i + 1])
                {
                    pair = tiles[i];
                    Console.WriteLine(new string(' ', recursionLevel * 2) + $"找到雀頭（對子）: {pair}");
                    return true;
                }
            }

            return false;
        }


        private static bool IsHonor(string type)
        {
            return type == "東" || type == "南" || type == "西" || type == "北" ||
                   type == "白" || type == "發" || type == "中";
        }


        // 判斷是否為數牌
        private static bool IsNumberTile(string tile)
        {
            return tile.EndsWith("萬") || tile.EndsWith("餅") || tile.EndsWith("索");
        }

        private static (int number, string type) GetNumberAndType(string tile)
        {
            if (tile.StartsWith("赤五"))
                return (5, tile.Substring(2));

            string[] numbers = { "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            for (int i = 0; i < numbers.Length; i++)
            {
                if (tile.StartsWith(numbers[i]))
                {
                    return (i + 1, tile.Substring(numbers[i].Length));
                }
            }

            return (0, tile);
        }


        private bool IsPinfu()//平和
        {
            if (discardedTiles.Any()) return false; // 必須門清

            var groups = playerHand.GroupBy(x => x.Replace("赤", ""));
            int shuntsuCount = 0;
            bool hasNonYakuhaiPair = false;
            bool hasTwoSidedWait = false;

            foreach (var group in groups)
            {
                if (group.Count() == 2) // 雀頭
                {
                    hasNonYakuhaiPair = !IsYakuhai(group.Key);
                }
                else if (IsShuntsu(group.Key)) // 順子
                {
                    shuntsuCount++;
                    // 檢查是否有兩面待ち
                    hasTwoSidedWait = true; // 簡化版本，實際應該檢查具體的待ち形
                }
            }

            return shuntsuCount == 4 && hasNonYakuhaiPair && hasTwoSidedWait;
        }

        private bool ContainsYaochu(string tile)
        {
            string normalizedTile = tile.Replace("赤", "");
            return normalizedTile.StartsWith("一") ||
                normalizedTile.StartsWith("九") ||
                IsJihai(normalizedTile);
        }
        private bool IsTanyao()//斷么九
        {
            return playerHand.All(tile => !IsYaochu(tile.Replace("赤", "")));
        }

        private bool IsIipeikou() //一盃口
        {
            // 先按花色分組
            var handBySuit = playerHand
                .GroupBy(t => GetSuit(t.Replace("赤", "")))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var suitGroup in handBySuit)
            {
                if (string.IsNullOrEmpty(suitGroup.Key)) continue;

                var tiles = suitGroup.Value.Select(t => t.Replace("赤", "")).ToList();
                var foundShuntsu = new List<(int start, string suit)>();

                // 尋找所有可能的順子
                for (int i = 1; i <= 7; i++)
                {
                    string tile1 = GetTileString(i, suitGroup.Key);
                    string tile2 = GetTileString(i + 1, suitGroup.Key);
                    string tile3 = GetTileString(i + 2, suitGroup.Key);

                    // 計算每個牌的數量
                    var remainingTiles = new List<string>(tiles);
                    bool foundFirst = false;

                    // 尋找第一組順子
                    if (remainingTiles.Contains(tile1) &&
                        remainingTiles.Contains(tile2) &&
                        remainingTiles.Contains(tile3))
                    {
                        foundFirst = true;
                        remainingTiles.Remove(tile1);
                        remainingTiles.Remove(tile2);
                        remainingTiles.Remove(tile3);

                        // 在剩餘的牌中尋找相同的順子
                        if (remainingTiles.Contains(tile1) &&
                            remainingTiles.Contains(tile2) &&
                            remainingTiles.Contains(tile3))
                        {
                            // 找到一盃口
                            return true;
                        }
                    }

                    if (!foundFirst)
                    {
                        continue;
                    }
                }
            }

            return false;
        }

        private bool IsIttsu() //一氣通貫
        {
            // 按花色分組
            var handBySuit = playerHand
                .GroupBy(t => GetSuit(t.Replace("赤", "")))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var suitGroup in handBySuit)
            {
                if (string.IsNullOrEmpty(suitGroup.Key)) continue;

                var tiles = suitGroup.Value.Select(t => t.Replace("赤", "")).ToList();
                var remainingTiles = new List<string>(tiles);

                // 檢查是否有 123
                if (!HasShuntsu(remainingTiles, 1, suitGroup.Key)) continue;

                // 檢查是否有 456
                if (!HasShuntsu(remainingTiles, 4, suitGroup.Key)) continue;

                // 檢查是否有 789
                if (!HasShuntsu(remainingTiles, 7, suitGroup.Key)) continue;

                return true;
            }

            return false;
        }

        // 輔助方法：檢查(((特定順子)))是否存在
        private bool HasShuntsu(List<string> tiles, int startNumber, string suit)
        {
            string tile1 = GetTileString(startNumber, suit);
            string tile2 = GetTileString(startNumber + 1, suit);
            string tile3 = GetTileString(startNumber + 2, suit);

            return tiles.Contains(tile1) &&
                   tiles.Contains(tile2) &&
                   tiles.Contains(tile3);
        }


        private bool IsChanta()//混全帶么九
        {
            if (IsJunchan() || IsHonroutou()) return false; // 不與純全帶么九和混老頭重複
            return playerHand.All(tile => ContainsYaochu(tile.Replace("赤", "")));
        }

        private bool IsChitoitsu()//七對子
        {
            var groups = playerHand.GroupBy(x => x.Replace("赤", ""));
            return groups.Count() == 7 && groups.All(g => g.Count() == 2);
        }

        private bool IsToitoi()//對對和
        {
            var groups = playerHand.GroupBy(x => x.Replace("赤", ""));
            return groups.All(g => g.Count() >= 3 || g.Count() == 2);
        }

        private bool IsSanankou()//三暗刻
        {
            var kotsuList = GetKotsuList();
            return kotsuList.Count >= 3;
        }

        private bool IsSanshokuDoukou()//三色同刻
        {
            var kotsuList = GetKotsuList();
            for (int i = 1; i <= 9; i++)
            {
                if (kotsuList.Contains($"{i}萬") &&
                    kotsuList.Contains($"{i}餅") &&
                    kotsuList.Contains($"{i}索"))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsHonroutou()//混老頭
        {
            return playerHand.All(tile =>
            {
                string normalizedTile = tile.Replace("赤", "");
                return normalizedTile.StartsWith("一") ||
                    normalizedTile.StartsWith("九") ||
                    IsJihai(normalizedTile);
            });
        }

        private bool IsShouSangen()//小三元
        {
            var groups = playerHand.GroupBy(x => x.Replace("赤", ""));
            int sangenCount = 0;
            foreach (var group in groups)
            {
                if (IsSangen(group.Key))
                {
                    if (group.Count() >= 3) sangenCount++;
                    else if (group.Count() == 2) return false;
                }
            }
            return sangenCount == 2;
        }

        private bool IsHonitsu()//混一色
        {
            if (IsChinitsu()) return false; // 不與清一色重複
            string suit = GetSuit(playerHand[0].Replace("赤", ""));
            return playerHand.All(tile =>
            {
                string normalizedTile = tile.Replace("赤", "");
                return GetSuit(normalizedTile) == suit || IsJihai(normalizedTile);
            });
        }

        private bool IsJunchan()//純全帶么九
        {
            return playerHand.All(tile =>
            {
                string normalizedTile = tile.Replace("赤", "");
                return normalizedTile.StartsWith("一") ||
                    normalizedTile.StartsWith("九");
            });
        }

        private bool IsRyanpeikou()//二盃口
        {
            if (discardedTiles.Any()) return false; // 必須門清
            var shuntsuList = GetShuntsuList();
            var groups = shuntsuList.GroupBy(x => x);
            return groups.Count(g => g.Count() >= 2) >= 2;
        }

        private bool IsChinitsu()  // 清一色
        {
            if (playerHand.Count == 0) return false;

            // 獲取第一張牌的花色（萬、筒、索）
            string firstTile = playerHand[0].Replace("*", "").Replace("赤", "");
            string suit = "";
            if (firstTile.Contains("萬")) suit = "萬";
            else if (firstTile.Contains("筒")) suit = "筒";
            else if (firstTile.Contains("索")) suit = "索";
            else return false;  // 如果是字牌則不可能是清一色

            // 檢查所有牌是否都是同一花色
            return playerHand.All(tile =>
            {
                string baseTile = tile.Replace("*", "").Replace("赤", "");
                return baseTile.Contains(suit);
            });
        }

        // 役滿判定
        private bool IsKokushi()//國士無雙
        {
            var yaochus = new HashSet<string>
            {
                "一萬", "九萬", "一餅", "九餅", "一索", "九索",
                "東", "南", "西", "北", "白", "發", "中"
            };
            var handTiles = new HashSet<string>(
                playerHand.Select(x => x.Replace("赤", "")));
            return handTiles.SetEquals(yaochus);
        }

        private bool IsSuuankou()//四暗刻
        {
            if (discardedTiles.Any()) return false;
            return GetKotsuList().Count == 4;
        }

        private bool IsDaisangen()//大三元
        {
            var groups = playerHand.GroupBy(x => x.Replace("赤", ""));
            int sangenCount = 0;
            foreach (var group in groups)
            {
                if (IsSangen(group.Key) && group.Count() >= 3)
                {
                    sangenCount++;
                }
            }
            return sangenCount == 3;
        }

        private bool IsShousuushi()//小四喜
        {
            var groups = playerHand.GroupBy(x => x.Replace("赤", ""));
            int windCount = 0;
            foreach (var group in groups)
            {
                if (IsWind(group.Key))
                {
                    if (group.Count() >= 3) windCount++;
                    else if (group.Count() == 2) return false;
                }
            }
            return windCount == 3;
        }

        private bool IsDaisuushi()//大四喜
        {
            var windTiles = new[] { "東", "南", "西", "北" };

            // 檢查每種風牌是否都有至少三張
            return windTiles.All(wind =>
            {
                var count = playerHand.Count(t =>
                    t.Replace("*", "").Replace("赤", "") == wind);
                return count >= 3;
            });
        }

        private bool IsRyuuiisou()//綠一色
        {
            // 定義綠一色可用的牌
            var greenTiles = new[] { "二索", "三索", "四索", "六索", "八索", "發" };

            // 檢查手牌中是否只包含綠一色可用的牌
            foreach (var tile in playerHand)
            {
                if (!greenTiles.Contains(tile))
                {
                    return false;
                }
            }

            // 檢查是否有至少一個對子或刻子
            var groups = playerHand.GroupBy(x => x);
            bool hasValidGroup = false;
            foreach (var group in groups)
            {
                if (group.Count() >= 2)  // 對子或刻子
                {
                    hasValidGroup = true;
                    break;
                }
            }

            return hasValidGroup;
        }

        private bool IsTsuuiisou()//字一色
        {
            return playerHand.All(tile =>
            {
                var baseTile = tile.Replace("*", "").Replace("赤", "");
                return baseTile == "東" || baseTile == "南" ||
                    baseTile == "西" || baseTile == "北" ||
                    baseTile == "白" || baseTile == "發" ||
                    baseTile == "中";
            });
        }

        private bool IsChinroutou()//清老頭
        {
            return playerHand.All(tile =>
            {
                string normalizedTile = tile.Replace("赤", "");
                return normalizedTile.StartsWith("一") ||
                    normalizedTile.StartsWith("九");
            });
        }

        private bool IsSuukantsu()//四槓子
        {
            // 計算槓牌組的數量
            var kangGroups = playerHand
                .Where(t => t.StartsWith("*"))
                .GroupBy(t => t.Replace("*", "").Replace("赤", ""))
                .Count();

            return kangGroups >= 4;
        }

        private bool IsChuurenpoutou()//九蓮寶燈
        {
            if (discardedTiles.Any()) return false;
            var normalizedHand = playerHand.Select(x => x.Replace("赤", "")).ToList();
            string suit = GetSuit(normalizedHand[0]);
            if (string.IsNullOrEmpty(suit)) return false;

            int[] requiredCounts = { 3, 1, 1, 1, 1, 1, 1, 1, 3 };
            string[] numbers = { "一", "二", "三", "四", "五", "六", "七", "八", "九" };

            for (int i = 0; i < 9; i++)
            {
                string tile = $"{numbers[i]}{suit}";
                if (normalizedHand.Count(x => x == tile) < requiredCounts[i])
                    return false;
            }

            return true;
        }

        private bool IsJunseiChuurenpoutou()//純正九蓮寶燈
        {
            if (!IsChuurenpoutou()) return false;

            var normalizedHand = playerHand.Select(x => x.Replace("赤", "")).ToList();
            string suit = GetSuit(normalizedHand[0]);

            int[] exactCounts = { 3, 1, 1, 1, 1, 1, 1, 1, 3 };
            string[] numbers = { "一", "二", "三", "四", "五", "六", "七", "八", "九" };

            for (int i = 0; i < 9; i++)
            {
                string tile = $"{numbers[i]}{suit}";
                if (normalizedHand.Count(x => x == tile) != exactCounts[i])
                    return false;
            }

            return true;
        }

        private bool IsKokushimusouJuusanmenmachi()//國士無雙十三面待ち
        {
            if (!IsKokushi()) return false;

            // 檢查是否所有么九牌都只有一張
            var yaochus = new HashSet<string>
        {
            "一萬", "九萬", "一餅", "九餅", "一索", "九索",
            "東", "南", "西", "北", "白", "發", "中"
        };

            foreach (var yaochu in yaochus)
            {
                if (playerHand.Count(tile => tile.Replace("赤", "") == yaochu) != 1)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSuankoTanki()//四暗刻單騎
        {
            if (!IsSuuankou()) return false;

            // 檢查是否只有一對雀頭
            var groups = playerHand.GroupBy(x => x.Replace("赤", ""));
            return groups.Count(g => g.Count() == 2) == 1;
        }

        private bool IsTenhou()//天和
        {
            // 天和需要在第一巡和牌且是莊家
            return isFirstRound && !discardedTiles.Any() && hasWon;
        }

        // 輔助方法
        private bool IsYakuhai(string tile)
        {
            return IsWind(tile) || IsSangen(tile);
        }

        private bool IsWind(string tile)
        {
            return new[] { "東", "南", "西", "北" }.Contains(tile);
        }

        private bool IsSangen(string tile)
        {
            return new[] { "白", "發", "中" }.Contains(tile);
        }

        private bool IsJihai(string tile)
        {
            return IsWind(tile) || IsSangen(tile);
        }

        private bool IsYaochu(string tile)
        {
            return tile.StartsWith("一") || tile.StartsWith("九") || IsJihai(tile);
        }

        private string GetSuit(string tile)
        {
            if (tile.EndsWith("萬")) return "萬";
            if (tile.EndsWith("餅")) return "餅";
            if (tile.EndsWith("索")) return "索";
            return "";
        }

        private bool IsShuntsu(string tile)
        {
            if (IsJihai(tile)) return false;
            string suit = GetSuit(tile);
            if (string.IsNullOrEmpty(suit)) return false;
            string number = tile.Replace(suit, "");
            return new[] { "一", "二", "三", "四", "五", "六", "七" }.Contains(number);
        }

        private List<string> GetShuntsuList()
        {
            var result = new List<string>();
            string[] suits = { "萬", "餅", "索" };
            string[] numbers = { "一", "二", "三", "四", "五", "六", "七" };

            foreach (var suit in suits)
            {
                foreach (var number in numbers)
                {
                    string tile1 = $"{number}{suit}";
                    string tile2 = $"{GetNextNumber(number)}{suit}";
                    string tile3 = $"{GetNextNumber(GetNextNumber(number))}{suit}";

                    if (playerHand.Contains(tile1) &&
                        playerHand.Contains(tile2) &&
                        playerHand.Contains(tile3))
                    {
                        result.Add(tile1);
                    }
                }
            }
            return result;
        }

        private static string GetNextNumber(string number)
        {
            string[] numbers = { "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            int index = Array.IndexOf(numbers, number);
            if (index == -1 || index == numbers.Length - 1) return number;
            return numbers[index + 1];
        }

        private List<string> GetKotsuList()
        {
            return playerHand
                .GroupBy(x => x.Replace("赤", ""))
                .Where(g => g.Count() >= 3)
                .Select(g => g.Key)
                .ToList();
        }
    }
}