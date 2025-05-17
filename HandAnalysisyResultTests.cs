using System;
using System.Collections.Generic;
using System.Linq;
using MahjongApp; // HandAnalysisResult などが含まれる名前空間

namespace MahjongApp // テスト実行用の名前空間 (任意)
{
    public class HandAnalysisResultTests
    {
        public void RunAllTests()
        {
            Console.WriteLine("Starting HandAnalysisResult Tests...\n");

            TestKokushiMusou_SingleWait_Tsumo();
            TestKokushiMusou_SingleWait_Ron();
            TestChiitoitsu_Tsumo();
            TestChiitoitsu_Ron();
            TestStandardHand_RyanmenMachi_Tsumo();
            TestStandardHand_WithPon_PenchanMachi_Ron();
            TestNotWinningHand_NoJantou();
            // TODO: 他のテストケースをここに追加して呼び出す

            Console.WriteLine("\nAll tests finished. Review the output above.");
        }

        // --- ヘルパーメソッド ---
        private Tile CreateTile(Suit suit, int number, int index = 0)
        {
            return new Tile(suit, number, index);
        }

        private string TilesToString(IEnumerable<Tile> tiles)
        {
            if (tiles == null || !tiles.Any()) return "[]";
            return "[" + string.Join(", ", tiles.Select(t => t.ToString())) + "]";
        }

        private string MeldsToString(IEnumerable<Meld> melds)
        {
            if (melds == null || !melds.Any()) return "[]";
            return "[\n  " + string.Join(",\n  ", melds.Select(m => $"{m.Type}: {TilesToString(m.Tiles)}")) + "\n]";
        }

        private void PrintAnalysisResult(HandAnalysisResult result, string testName)
        {
            Console.WriteLine($"--- Test Case: {testName} ---");
            if (result == null)
            {
                Console.WriteLine("Error: HandAnalysisResult is null.");
                Console.WriteLine("--- End Test Case ---");
                return;
            }

            Console.WriteLine($"  IsWinningHand: {result.IsWinningHand}");
            Console.WriteLine($"  FormType: {result.FormType}");
            Console.WriteLine($"  IsTsumo: {result.IsTsumo}");
            Console.WriteLine($"  IsMenzen: {result.IsMenzen}");
            Console.WriteLine($"  WinningTile: {result.WinningTile?.ToString() ?? "N/A"}");
            Console.WriteLine($"  Jantou: {result.Jantou?.ToString() ?? "N/A"}");
            Console.WriteLine($"  MentsuList: {MeldsToString(result.MentsuList)}");
            Console.WriteLine($"  AnalyzedHand: {TilesToString(result.AnalyzedHand)}");
            Console.WriteLine($"  IsRyanmenMachi: {result.IsRyanmenMachi}");
            Console.WriteLine($"--- End Test Case: {testName} ---\n");
        }

        // --- アサーションヘルパー (手動確認用) ---
        private bool AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ASSERTION FAILED: {message}");
                Console.ResetColor();
                return false;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"ASSERTION PASSED: {message}");
            Console.ResetColor();
            return true;
        }


        private bool AssertEquals<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ASSERTION FAILED: {message}. Expected: {expected}, Actual: {actual}");
                Console.ResetColor();
                return false;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"ASSERTION PASSED: {message}. Expected: {expected}, Actual: {actual}");
            Console.ResetColor();
            return true;
        }


        // --- テストケースメソッド ---

        public void TestKokushiMusou_SingleWait_Tsumo()
        {
            string testName = "KokushiMusou_SingleWait_Tsumo";
            var player = new Player();
            player.Hand.AddRange(new List<Tile>
            {
                CreateTile(Suit.Manzu, 1, 1), CreateTile(Suit.Manzu, 9, 1),
                CreateTile(Suit.Pinzu, 1, 1), CreateTile(Suit.Pinzu, 9, 1),
                CreateTile(Suit.Souzu, 1, 1), CreateTile(Suit.Souzu, 9, 1),
                CreateTile(Suit.Honor, 1, 1), CreateTile(Suit.Honor, 2, 1),
                CreateTile(Suit.Honor, 3, 1), CreateTile(Suit.Honor, 4, 1),
                CreateTile(Suit.Honor, 5, 1), CreateTile(Suit.Honor, 6, 1),
                CreateTile(Suit.Honor, 7, 1) // 13枚
            });
            var winningTile = CreateTile(Suit.Manzu, 1, 2); // 和了牌

            player.Hand.Add(winningTile); // 和了牌を手牌に追加

            Console.WriteLine($"Player Hand for {testName}: {TilesToString(player.Hand)}");
            Console.WriteLine($"Winning Tile for {testName}: {winningTile}");

            var analysisResult = new HandAnalysisResult(player, winningTile, true); // isTsumo = true
            PrintAnalysisResult(analysisResult, testName);

            // 手動アサーション
            AssertTrue(analysisResult.IsWinningHand, $"{testName}: Should be a winning hand.");
            AssertEquals(WinningFormType.KokushiMusou, analysisResult.FormType, $"{testName}: FormType should be KokushiMusou.");
            AssertTrue(analysisResult.IsTsumo, $"{testName}: Should be Tsumo.");
            AssertTrue(analysisResult.IsMenzen, $"{testName}: Should be Menzen.");
        }
        public void TestKokushiMusou_SingleWait_Ron()
        {
            string testName = "KokushiMusou_SingleWait_Ron";
            var player = new Player();
            player.Hand.AddRange(new List<Tile>
            {
                CreateTile(Suit.Manzu, 1, 1), CreateTile(Suit.Manzu, 9, 1),
                CreateTile(Suit.Pinzu, 1, 1), CreateTile(Suit.Pinzu, 9, 1),
                CreateTile(Suit.Souzu, 1, 1), CreateTile(Suit.Souzu, 9, 1),
                CreateTile(Suit.Honor, 1, 1), CreateTile(Suit.Honor, 2, 1),
                CreateTile(Suit.Honor, 3, 1), CreateTile(Suit.Honor, 4, 1),
                CreateTile(Suit.Honor, 5, 1), CreateTile(Suit.Honor, 6, 1),
                CreateTile(Suit.Honor, 7, 1) // 13枚
            });
            var winningTile = CreateTile(Suit.Manzu, 1, 2); // 和了牌

            Console.WriteLine($"Player Hand for {testName}: {TilesToString(player.Hand)}");
            Console.WriteLine($"Winning Tile for {testName}: {winningTile}");

            var analysisResult = new HandAnalysisResult(player, winningTile, false); // isTsumo = true
            PrintAnalysisResult(analysisResult, testName);

            // 手動アサーション
            AssertTrue(analysisResult.IsWinningHand, $"{testName}: Should be a winning hand.");
            AssertEquals(WinningFormType.KokushiMusou, analysisResult.FormType, $"{testName}: FormType should be KokushiMusou.");
            AssertTrue(!analysisResult.IsTsumo, $"{testName}: Should be Ron (not Tsumo).");
            AssertTrue(analysisResult.IsMenzen, $"{testName}: Should be Menzen.");
        }

        public void TestChiitoitsu_Tsumo()
        {
            string testName = "Chiitoitsu_Tsumo";
            var player = new Player();
            player.Hand.AddRange(new List<Tile>
            {
                CreateTile(Suit.Manzu, 1, 1), CreateTile(Suit.Manzu, 1, 2),
                CreateTile(Suit.Pinzu, 2, 1), CreateTile(Suit.Pinzu, 2, 2),
                CreateTile(Suit.Souzu, 3, 1), CreateTile(Suit.Souzu, 3, 2),
                CreateTile(Suit.Honor, 1, 1), CreateTile(Suit.Honor, 1, 2),
                CreateTile(Suit.Honor, 2, 1), CreateTile(Suit.Honor, 2, 2),
                CreateTile(Suit.Honor, 3, 1), CreateTile(Suit.Honor, 3, 2),
                CreateTile(Suit.Manzu, 5,1) // 13枚の手牌
            });
            var winningTile = CreateTile(Suit.Manzu, 5, 2); // 和了牌 (ロン)

            player.Hand.Add(winningTile); // 和了牌を手牌に追加

            Console.WriteLine($"Player Hand for {testName}: {TilesToString(player.Hand)}");
            Console.WriteLine($"Winning Tile for {testName}: {winningTile}");

            var analysisResult = new HandAnalysisResult(player, winningTile, true); // isTsumo = true
            PrintAnalysisResult(analysisResult, testName);

            AssertTrue(analysisResult.IsWinningHand, $"{testName}: Should be a winning hand.");
            AssertEquals(WinningFormType.Chiitoitsu, analysisResult.FormType, $"{testName}: FormType should be Chiitoitsu.");
            AssertTrue(analysisResult.IsTsumo, $"{testName}: Should be Tsumo.");
            AssertTrue(analysisResult.IsMenzen, $"{testName}: Should be Menzen.");
            // 七対子の場合、AnalyzedHandには14枚の牌が含まれ、7つの対になっているはず
            AssertEquals(14, analysisResult.AnalyzedHand.Count, $"{testName}: AnalyzedHand should contain 14 tiles.");
            AssertEquals(7, analysisResult.AnalyzedHand.GroupBy(t => $"{t.Suit}_{t.Number}").Count(g => g.Count() == 2), $"{testName}: AnalyzedHand should form 7 pairs.");
        }
        public void TestChiitoitsu_Ron()
        {
            string testName = "Chiitoitsu_Ron";
            var player = new Player();
            player.Hand.AddRange(new List<Tile>
            {
                CreateTile(Suit.Manzu, 1, 1), CreateTile(Suit.Manzu, 1, 2),
                CreateTile(Suit.Pinzu, 2, 1), CreateTile(Suit.Pinzu, 2, 2),
                CreateTile(Suit.Souzu, 3, 1), CreateTile(Suit.Souzu, 3, 2),
                CreateTile(Suit.Honor, 1, 1), CreateTile(Suit.Honor, 1, 2),
                CreateTile(Suit.Honor, 2, 1), CreateTile(Suit.Honor, 2, 2),
                CreateTile(Suit.Honor, 3, 1), CreateTile(Suit.Honor, 3, 2),
                CreateTile(Suit.Manzu, 5,1) // 13枚の手牌
            });
            var winningTile = CreateTile(Suit.Manzu, 5, 2); // 和了牌 (ロン)

            Console.WriteLine($"Player Hand for {testName}: {TilesToString(player.Hand)}");
            Console.WriteLine($"Winning Tile for {testName}: {winningTile}");

            var analysisResult = new HandAnalysisResult(player, winningTile, false); // isTsumo = false
            PrintAnalysisResult(analysisResult, testName);

            AssertTrue(analysisResult.IsWinningHand, $"{testName}: Should be a winning hand.");
            AssertEquals(WinningFormType.Chiitoitsu, analysisResult.FormType, $"{testName}: FormType should be Chiitoitsu.");
            AssertTrue(!analysisResult.IsTsumo, $"{testName}: Should be Ron (not Tsumo).");
            AssertTrue(analysisResult.IsMenzen, $"{testName}: Should be Menzen.");
            // 七対子の場合、AnalyzedHandには14枚の牌が含まれ、7つの対になっているはず
            AssertEquals(14, analysisResult.AnalyzedHand.Count, $"{testName}: AnalyzedHand should contain 14 tiles.");
            AssertEquals(7, analysisResult.AnalyzedHand.GroupBy(t => $"{t.Suit}_{t.Number}").Count(g => g.Count() == 2), $"{testName}: AnalyzedHand should form 7 pairs.");
        }

        public void TestStandardHand_RyanmenMachi_Tsumo()
        {
            string testName = "StandardHand_RyanmenMachi_Tsumo";
            var player = new Player();
            // 手牌: 123m 456p 789s 東東 34m (和了牌 2m or 5m) - 今回は5mツモ
            player.Hand.AddRange(new List<Tile>
            {
                CreateTile(Suit.Manzu, 1), CreateTile(Suit.Manzu, 2), CreateTile(Suit.Manzu, 3),
                CreateTile(Suit.Pinzu, 4), CreateTile(Suit.Pinzu, 5), CreateTile(Suit.Pinzu, 6),
                CreateTile(Suit.Souzu, 7), CreateTile(Suit.Souzu, 8), CreateTile(Suit.Souzu, 9),
                CreateTile(Suit.Honor, 1, 1), CreateTile(Suit.Honor, 1, 2), // 東東 (雀頭)
                CreateTile(Suit.Manzu, 3, 1), CreateTile(Suit.Manzu, 4, 1)  // 34m (待ち)
            });
            var winningTile = CreateTile(Suit.Manzu, 5, 1); // 和了牌 (ツモ)

            player.Hand.Add(winningTile); // 和了牌を手牌に追加

            Console.WriteLine($"Player Hand for {testName}: {TilesToString(player.Hand)}");
            Console.WriteLine($"Winning Tile for {testName}: {winningTile}");

            var analysisResult = new HandAnalysisResult(player, winningTile, true);
            PrintAnalysisResult(analysisResult, testName);

            AssertTrue(analysisResult.IsWinningHand, $"{testName}: Should be a winning hand.");
            AssertEquals(WinningFormType.Standard, analysisResult.FormType, $"{testName}: FormType should be Standard.");
            AssertTrue(analysisResult.IsTsumo, $"{testName}: Should be Tsumo.");
            AssertTrue(analysisResult.IsMenzen, $"{testName}: Should be Menzen.");
            AssertTrue(analysisResult.Jantou != null && analysisResult.Jantou.Suit == Suit.Honor && analysisResult.Jantou.Number == 1, $"{testName}: Jantou should be East.");
            AssertEquals(4, analysisResult.MentsuList.Count, $"{testName}: Should have 4 melds.");
            // IsRyanmenMachiの検証はHandAnalysisResultの実装に依存します
            // このケースでは、345mの順子が完成し、元々2mと5mの両面待ちだったのでtrueを期待
            // ただし、CheckRyanmenMachiの現在のロジックでは、「雀頭が順子の一部」という判定のため、
            // このケースでIsRyanmenMachiがどうなるかは注意が必要です。
            // `HandAnalysisResult`内の`CheckRyanmenMachi`がどのように呼び出され、どの面子で評価されるかによります。
            // `FindStandardHand`内で見つかった面子に対して`CheckRyanmenMachi`が呼ばれると仮定。
            // ここでは、`winningTile`を含む完成した面子で判定されることを期待します。
            AssertTrue(analysisResult.IsRyanmenMachi, $"{testName}: Should be RyanmenMachi.");
        }

        public void TestStandardHand_WithPon_PenchanMachi_Ron()
        {
            string testName = "StandardHand_WithPon_PenchanMachi_Ron";
            var player = new Player();
            var ponMeld = new Meld { Type = MeldType.Pon, Tiles = new List<Tile> { CreateTile(Suit.Honor, 6, 1), CreateTile(Suit.Honor, 6, 2), CreateTile(Suit.Honor, 6, 3) } }; // 中のポン
            player.Melds.Add(ponMeld);

            // 手牌: 12m(待ち), 567p, 白白(雀頭)  + ポン(中) => 和了牌3m (ロン)
            player.Hand.AddRange(new List<Tile>
            {
                CreateTile(Suit.Pinzu, 5), CreateTile(Suit.Pinzu, 6), CreateTile(Suit.Pinzu, 7),
                CreateTile(Suit.Pinzu, 5), CreateTile(Suit.Pinzu, 6), CreateTile(Suit.Pinzu, 7),
                CreateTile(Suit.Honor, 5, 1), CreateTile(Suit.Honor, 5, 2), // 白白 (雀頭)
                CreateTile(Suit.Manzu, 1), CreateTile(Suit.Manzu, 2)       // 12m (待ち)
            });
            var winningTile = CreateTile(Suit.Manzu, 3); // 和了牌 (ロン)

            Console.WriteLine($"Player Hand for {testName}: {TilesToString(player.Hand)}");
            Console.WriteLine($"Player Melds for {testName}: {MeldsToString(player.Melds)}");
            Console.WriteLine($"Winning Tile for {testName}: {winningTile}");

            var analysisResult = new HandAnalysisResult(player, winningTile, false); // isTsumo = false
            PrintAnalysisResult(analysisResult, testName);

            AssertTrue(analysisResult.IsWinningHand, $"{testName}: Should be a winning hand.");
            AssertEquals(WinningFormType.Standard, analysisResult.FormType, $"{testName}: FormType should be Standard.");
            AssertTrue(!analysisResult.IsTsumo, $"{testName}: Should be Ron (not Tsumo).");
            AssertTrue(!analysisResult.IsMenzen, $"{testName}: Should not be Menzen due to Pon.");
            AssertTrue(analysisResult.Jantou != null && analysisResult.Jantou.Suit == Suit.Honor && analysisResult.Jantou.Number == 5, $"{testName}: Jantou should be Haku (White Dragon)."); // 白はHonorの5番目と仮定
            AssertEquals(4, analysisResult.MentsuList.Count, $"{testName}: Should have 4 melds (1 pon + 3 from hand).");
            AssertTrue(analysisResult.MentsuList.Any(m => m.Type == MeldType.Pon), $"{testName}: MentsuList should contain the Pon.");
            // ペンチャン待ちは両面待ちではない
            AssertTrue(!analysisResult.IsRyanmenMachi, $"{testName}: Should not be RyanmenMachi.");
        }

        public void TestNotWinningHand_NoJantou()
        {
            string testName = "NotWinningHand_NoJantou";
            var player = new Player();

            // 123m 456p 789s 123s 4s (雀頭なし)
            player.Hand.AddRange(new List<Tile>
            {
                CreateTile(Suit.Manzu, 1), CreateTile(Suit.Manzu, 2), CreateTile(Suit.Manzu, 3),
                CreateTile(Suit.Pinzu, 4), CreateTile(Suit.Pinzu, 5), CreateTile(Suit.Pinzu, 6),
                CreateTile(Suit.Souzu, 7), CreateTile(Suit.Souzu, 8), CreateTile(Suit.Souzu, 9),
                CreateTile(Suit.Souzu, 1,1), CreateTile(Suit.Souzu, 2,1), CreateTile(Suit.Souzu, 3,1),
                CreateTile(Suit.Souzu, 4,1) // 13枚
            });
            var winningTile = CreateTile(Suit.Manzu, 5); // 仮の和了牌

            player.Hand.Add(winningTile); // 和了牌を手牌に追加

            Console.WriteLine($"Player Hand for {testName}: {TilesToString(player.Hand)}");
            Console.WriteLine($"Winning Tile for {testName}: {winningTile}");

            var analysisResult = new HandAnalysisResult(player, winningTile, true);
            PrintAnalysisResult(analysisResult, testName);

            AssertTrue(!analysisResult.IsWinningHand, $"{testName}: Should not be a winning hand.");
            AssertEquals(WinningFormType.None, analysisResult.FormType, $"{testName}: FormType should be None.");
        }
    }
}