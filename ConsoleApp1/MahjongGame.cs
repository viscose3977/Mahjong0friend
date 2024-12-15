using System;
using System.Collections.Generic;
using System.Linq;

namespace MahjongGame
{
    public partial class MahjongGame
    {
        // 核心數據
        private List<string> deck; // 牌山
        private List<string> playerHand; // 玩家手牌
        private List<string> doraIndicators; // 寶牌指示牌
        private List<string> uraDoraIndicators; // 裏寶牌指示牌
        private List<string> discardedTiles; // 已打出牌
        private List<string> firstRoundWindTiles; // 第一巡風牌
        private List<string> winningRecord; // 和牌紀錄
        private List<string> kangTiles = new List<string>();  // 儲存已槓的牌
        private List<string> markedTiles = new List<string>(); // 儲存被標記的牌（槓牌組中的三張）
        private List<string> yakumanYakuList; // 役滿役種
        private List<string> initialDoraIndicators = new List<string>();  // 保存初始的寶牌指示牌

        // 遊戲狀態
        private Random random; // 隨機數生成器
        private int playerPoints; // 玩家點數
        private bool hasWon; // 是否和牌
        private int richiCount; // 立直次數
        private int kangCount; // 槓牌次數
        private bool isFirstRound; // 是否為第一巡
        private int level; // 關卡
        private bool levelCompleted;
        private bool isRichi; //是否立直
        private bool isDoubleRiichi = false; // 是否雙立直

        public MahjongGame()
        {
            try
            {
                Console.WriteLine("初始化遊戲...");
                InitializeGameState();
                InitializeDeck();
                Console.WriteLine($"牌山初始化完成，共 {deck.Count} 張牌");
                ShuffleDeck();
                Console.WriteLine("洗牌完成");
                DealInitialHand();

                // 測試用，不用時註解掉
                playerHand = new List<string> { "東", "東", "東", "南", "南", "南", "西", "西", "西", "北", "北", "北", "中", "中" };
                SortHand();

                Console.WriteLine("發牌完成");
                GenerateDoraIndicators();
                Console.WriteLine("寶牌指示牌生成完成");
                Console.WriteLine("遊戲初始化完成！\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化過程中發生錯誤：{ex.Message}");
                throw;
            }
        }

        private class GameState
        {
            public List<string> PlayerHand { get; set; }
            public List<string> DiscardedTiles { get; set; }
            public int PlayerPoints { get; set; }
            public bool IsRichi { get; set; }
            public bool HasWon { get; set; }
            public int RichiCount { get; set; }
            public int KangCount { get; set; }
            public bool IsFirstRound { get; set; }
            public int Level { get; set; }
            public bool LevelCompleted { get; set; }
            public bool IsDoubleRiichi { get; set; }
        }

        private Stack<GameState> gameStateHistory = new Stack<GameState>();

        private void InitializeGameState()
        {
            random = new Random();
            playerPoints = 25000;
            isRichi = false;
            hasWon = false;
            richiCount = 0;
            kangCount = 0;
            isFirstRound = true;
            level = 1;
            levelCompleted = false;

            deck = new List<string>();
            playerHand = new List<string>();
            doraIndicators = new List<string>();
            uraDoraIndicators = new List<string>();
            discardedTiles = new List<string>();
            firstRoundWindTiles = new List<string>();
            winningRecord = new List<string>();
        }

        private void DisplayGameStatus()
        {
            Console.Clear();
            Console.WriteLine($"\n關卡：{level} - {GetLevelObjective()}");
            Console.WriteLine($"目前點數：{playerPoints}");
            Console.WriteLine($"剩餘牌數：{deck.Count}");
            Console.WriteLine($"立直狀態：{(isRichi ? "已立直" : "未立直")}");
            Console.WriteLine($"槓牌數量：{kangCount}");

            // 顯示寶牌指示牌
            string doraDisplay = doraIndicators.Count > 0 ? doraIndicators[0] : "無";
            string additionalDora = string.Join(" ", doraIndicators.Skip(1).Take(kangCount));
            string hiddenDora = string.Join(" ", Enumerable.Repeat("#", Math.Max(0, 5 - (kangCount + 1))));
            Console.WriteLine($"明寶牌指示牌：{doraDisplay} {additionalDora} {hiddenDora}");

            // 顯示裏寶牌指示牌（只有在立直和牌後才能翻開）
            if (isRichi && hasWon)
            {
                string uraDoraDisplay = string.Join(" ", uraDoraIndicators.Take(kangCount + 1));
                Console.WriteLine($"裏寶牌指示牌：{uraDoraDisplay}");
            }
            else
            {
                string hiddenUraDora = string.Join(" ", Enumerable.Repeat("#", kangCount + 1));
                Console.WriteLine($"裏寶牌指示牌：{hiddenUraDora}");
            }

            // 顯示已槓的牌
            Console.WriteLine("\n已槓牌：" + (kangTiles.Any() ? string.Join(" ", kangTiles) : "無"));

            // 顯示手牌
            Console.WriteLine("\n目前手牌：");

            int currentIndex = 1;  // 用於追踪整體的編號

            // 找出所有標記過的牌組
            var markedGroups = playerHand
                .Where(t => t.StartsWith("*"))
                .GroupBy(t => t.Replace("*", "").Replace("赤", ""))
                .ToList();

            // 顯示標記過的牌組（用括號包起來）
            foreach (var group in markedGroups)
            {
                var tiles = group.ToList();
                Console.Write($"({string.Join(" ", tiles.Select(t => $"{currentIndex++}.{t.Replace("*", "")}"))}) ");
            }

            // 顯示未標記的牌
            var unmarkedTiles = playerHand.Where(t => !t.StartsWith("*")).ToList();
            if (unmarkedTiles.Any())
            {
                Console.Write(string.Join(" ", unmarkedTiles.Select(t => $"{currentIndex++}.{t}")));
            }

            Console.WriteLine();

            if (discardedTiles.Any())
            {
                Console.WriteLine("\n已打出的牌：");
                // 統計重複的牌，並特別處理赤牌
                var groupedDiscards = discardedTiles
                    .GroupBy(t => t.Replace("赤", ""))  // 把赤牌和普通牌分在同一組
                    .Select(g => {
                        int normalCount = g.Count(t => !t.Contains("赤"));
                        int redCount = g.Count(t => t.Contains("赤"));

                        if (redCount > 0)
                        {
                            if (normalCount > 0)
                            {
                                return $"{g.Key}(*{normalCount}+赤)";
                            }
                            return $"{g.Key}(赤)";
                        }
                        else if (normalCount > 1)
                        {
                            return $"{g.Key}(*{normalCount})";  // 修改這裡，加上括號
                        }
                        return g.Key;
                    })
                    .ToList();
                Console.WriteLine(string.Join(" ", groupedDiscards));
            }
            Console.WriteLine();
        }
        public void PlayGame()
        {
            bool gameRunning = true;
            while (gameRunning && deck.Count > 0)
            {
                // 顯示當前遊戲狀態
                DisplayGameStatus();

                // 顯示可用操作
                Console.WriteLine("請選擇操作：");
                Console.WriteLine("1. 打出牌");
                Console.WriteLine("2. 立直");
                Console.WriteLine("3. 胡牌");
                Console.WriteLine("4. 槓");
                if (levelCompleted)
                {
                    Console.WriteLine("5. 進入下一關");
                }

                // 讀取玩家輸入
                Console.Write("請選擇操作，按0返回上一步: ");
                string input = Console.ReadLine();
                if (!int.TryParse(input, out int choice))
                {
                    Console.WriteLine("無效的輸入，請重試。");
                    continue;
                }
                // 處理玩家選擇
                switch (choice)
                {
                    case 1:
                        DiscardTile();
                        break;
                    case 2:
                        DeclareRiichi();
                        break;
                    case 3: // 和牌
                        try
                        {
                            int han = CalculateHan();
                            if (han > 0)
                            {
                                CompleteHand();
                            }
                            else
                            {
                                Console.WriteLine("目前無法和牌，請確認手牌狀態。");
                                Console.WriteLine();
                                InvalidWinAttempt();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"和牌判定時發生錯誤：{ex.Message}");
                        }
                        break;
                    case 4: // 槓牌
                        DeclareKang();  // 直接調用 DeclareKang，讓它處理所有槓牌相關的邏輯
                        break;
                    case 5: // 下一關卡
                        if (levelCompleted)
                        {
                            Console.WriteLine("\n正在初始化下一關卡...");
                            Console.WriteLine("\n按任意鍵繼續...");
                            Console.ReadKey();
                            level++;
                            levelCompleted = false;
                            InitializeNewLevel();
                        }
                        break;
                    default:
                        Console.WriteLine("無效的選擇，請重試。");
                        break;
                }
                if (deck.Count == 0)
                {
                    Console.WriteLine("\n牌山為零，本局已結束！");
                    Console.WriteLine($"本局{GetLevelObjective()}");
                    Console.WriteLine();
                    Console.WriteLine($"當前點數：{playerPoints}");
                    Console.WriteLine("和牌紀錄：");
                    Console.WriteLine();
                    if (winningRecord.Any())
                    {
                        foreach (var record in winningRecord)
                        {
                            Console.WriteLine(record);
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine("無");
                        Console.WriteLine();
                    }

                    Console.WriteLine($"通關狀態：{(levelCompleted ? "已達成" : "未達成")}");
                    Console.WriteLine();

                    // 檢查是否達成通關條件
                    if (!levelCompleted)
                    {
                        Console.WriteLine("\n偵測到玩家未達成通關條件，獲得成就：【菜就多練】");
                        Console.WriteLine("遊戲失敗，即將返回主畫面...");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    else
                    {
                        // 已達成通關條件，詢問是否進入下一關
                        Console.WriteLine("\n是否進入下一關卡？(Y/N)");
                        string nextLevelChoice = Console.ReadLine().ToUpper();

                        if (nextLevelChoice == "Y")
                        {
                            level++; // 進入下一關
                            ResetGame();
                            PlayGame();  // 直接開始新的一局
                            return;
                        }
                        else
                        {
                            Console.WriteLine("\n感謝遊玩，即將退出遊戲...");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                    }
                }
            }
        }



        private void ShowGameResult()
        {
            Console.WriteLine("本局已結束！");
            Console.WriteLine($"本局{GetLevelObjective()}");
            Console.WriteLine();
            Console.WriteLine($"當前點數：{playerPoints}");
            Console.WriteLine("和牌紀錄：");
            Console.WriteLine();
            if (winningRecord.Any())
            {
                foreach (var record in winningRecord)
                {
                    Console.WriteLine(record);
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("無");
                Console.WriteLine();
            }
        }
        // 新增重置遊戲的方法
        private void ResetGame()
        {
            // 重新初始化牌山
            InitializeDeck();
            ShuffleDeck();

            // 清空手牌和其他牌組
            playerHand.Clear();
            discardedTiles.Clear();
            doraIndicators.Clear();
            uraDoraIndicators.Clear();
            kangTiles.Clear();
            winningRecord.Clear();

            // 重新發牌
            DealInitialHand();
            GenerateDoraIndicators();

            // 重置遊戲狀態
            isRichi = false;
            hasWon = false;
            kangCount = 0;
            isFirstRound = true;
        }

        // 新增初始化新關卡的方法
        private void InitializeNewLevel()
        {
            // 保留點數，重置其他狀態
            ResetGame();
            Console.WriteLine($"\n進入第 {level} 關！");
        }


        private string GetLevelObjective()
        {
            switch (level)
            {
                case 1:
                    return "通關目標：三番30符起！";
                case 2:
                    return "通關目標：滿貫（4番40符起）！";
                case 3:
                    return "通關目標：跳滿（六番起）！";
                case 4:
                    return "通關目標：倍滿（八番起）！";
                case 5:
                    return "通關目標：三倍滿（十一番起）！";
                case 6:
                    return "通關目標：役滿（十三番起）！";
                default:
                    return "未知的關卡目標";
            }
        }
    }
}