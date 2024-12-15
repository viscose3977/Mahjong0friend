//DeckManagement.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace MahjongGame
{
    public partial class MahjongGame
    {
        private void InitializeDeck()
        {
            string[] suits = {
                    "一萬", "二萬", "三萬", "四萬", "五萬", "六萬", "七萬", "八萬", "九萬",
                    "一餅", "二餅", "三餅", "四餅", "五餅", "六餅", "七餅", "八餅", "九餅",
                    "一索", "二索", "三索", "四索", "五索", "六索", "七索", "八索", "九索",
                    "東", "南", "西", "北", "白", "發", "中"
            };

            //string[] suits = {
            //"東", "南", "西", "北", "中", "發", "白",
            //"東", "南", "西", "北", "中", "發", "白",
            //"東", "南", "西", "北", "中", "發", "白",
            //"東", "南", "西", "北", "中", "發", "白",
            //"東", "南", "西", "北", "中", "發" };

            deck.Clear();
            foreach (string tile in suits)
            {
                int tileCount = (tile == "五萬" || tile == "五餅" || tile == "五索") ? 3 : 4;
                for (int j = 0; j < tileCount; j++)
                {
                    deck.Add(tile);
                }
            }

            // 添加赤寶牌
            deck.Add("赤五萬");
            deck.Add("赤五餅");
            deck.Add("赤五索");
        }

        private void ShuffleDeck()
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]); // 交換牌的位置
            }
        }

        private void DealInitialHand()
        {
            playerHand.Clear();
            for (int i = 0; i < 14; i++)
            {
                playerHand.Add(deck[0]);
                deck.RemoveAt(0);
            }
            SortHand();
        }

        private void GenerateDoraIndicators()
        {
            doraIndicators.Clear();
            uraDoraIndicators.Clear();


            for (int i = 0; i < 5; i++)
            {

                doraIndicators.Add(deck[0]);
                deck.RemoveAt(0);

                uraDoraIndicators.Add(deck[0]);
                deck.RemoveAt(0);
            }
        }

        private void SortHand()
        {
            playerHand = playerHand
                .OrderBy(tile => GetTileSortOrder(tile.Replace("赤", "")))
                .ThenBy(tile => tile.Contains("赤") ? 0 : 1)
                .ToList();
        }

        private int GetTileSortOrder(string tile)
        {
            string[] manzu = { "一萬", "二萬", "三萬", "四萬", "五萬", "六萬", "七萬", "八萬", "九萬" };
            string[] pinzu = { "一餅", "二餅", "三餅", "四餅", "五餅", "六餅", "七餅", "八餅", "九餅" };
            string[] souzu = { "一索", "二索", "三索", "四索", "五索", "六索", "七索", "八索", "九索" };
            string[] winds = { "東", "南", "西", "北" };
            string[] dragons = { "白", "發", "中" };

            if (manzu.Contains(tile)) return Array.IndexOf(manzu, tile);
            if (pinzu.Contains(tile)) return 10 + Array.IndexOf(pinzu, tile);
            if (souzu.Contains(tile)) return 20 + Array.IndexOf(souzu, tile);
            if (winds.Contains(tile)) return 30 + Array.IndexOf(winds, tile);
            if (dragons.Contains(tile)) return 40 + Array.IndexOf(dragons, tile);
            return 50;
        }
    }
}