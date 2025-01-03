// Main.cs
using System;

public class Program  
{
    public static void Main(string[] args)
    {
        while (true)
        {
            try
            {
                Console.Clear();
                ShowMainMenu();

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Clear();
                        var game = new MahjongGame.MahjongGame();  // 使用完整的命名空間路徑
                        game.PlayGame();
                        break;
                    case "2":
                        Console.WriteLine("\n感謝遊玩，再見！");
                        Console.ReadKey();
                        return;
                    default:
                        Console.WriteLine("\n無效的選擇，請重試...");
                        Console.ReadKey();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"遊戲發生錯誤：{ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("\n按任意鍵返回主畫面...");
                Console.ReadKey();
            }
        }
    }


    private static void ShowMainMenu()
    {
        Console.WriteLine();
        Console.WriteLine("   _____     _____     _____     _____");
        Console.WriteLine("  /_____/|  /_____/|  /_____/|  /_____/|");
        Console.WriteLine(" |     | | |     | | |     | | |     | |");
        Console.WriteLine(" | 孤  | | | 兒  | | | 日  | | | 麻  | |");
        Console.WriteLine(" |     | | |     | | |     | | |     | |");
        Console.WriteLine(" |-----|/  |-----|/  |-----|/  |-----|/");
        Console.WriteLine("─歡迎遊玩孤兒日麻遊戲─");
        Console.WriteLine("遊戲目標：你將獨自一人摸打牌（因為你沒朋友），並且需要完成每道關卡的指定和牌要求");
        Console.WriteLine("\n");
        Console.WriteLine("操作說明：\n");
        Console.WriteLine("0. 返回：返回上一步");
        Console.WriteLine("1. 出牌：選擇要打出的牌");
        Console.WriteLine("2. 立直：聽牌後可宣告立直，和牌後可多立直役，立直時需支付1000點");
        Console.WriteLine("3. 和牌：達成和牌條件時可以和牌");
        Console.WriteLine("4. 槓：擁有四張相同的牌時可以槓");
        Console.WriteLine("\n");
        Console.WriteLine("規則說明：\n");
        Console.WriteLine("1. 本遊戲不提供相關教學，請自行查閱");
        Console.WriteLine("2. 假立直、假槓牌、詐和都將進行罰符");
        Console.WriteLine("3. 我超忙，做得很爛就是很爛，我有躁鬱症，你罵我我會玻璃心碎然後噴死你");
        Console.WriteLine("\n");
        Console.WriteLine("1. 開始遊戲");
        Console.WriteLine("2. 退出遊戲");
        Console.Write("請選擇：");
    }
}