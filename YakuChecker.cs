// // MahjongApp/YakuChecker.cs
// using System;
// using System.Collections.Generic;
// using System.Linq;

// namespace MahjongApp
// {
//     public class Yaku
//     {
//         public string Name { get; set; }
//         public int Han { get; set; }
//         public int HanClosedOnly { get; set; } // 食い下がりを考慮する場合

//         public Yaku(string name, int han, int hanClosedOnly = 0)
//         {
//             Name = name;
//             Han = han;
//             HanClosedOnly = hanClosedOnly > 0 ? hanClosedOnly : han;
//         }
//     }

//     // 役判定に必要なゲーム状況をまとめるクラス（例）
//     public class GameContext
//     {
//         public Wind RoundWind { get; set; } // 場風
//         public Wind PlayerWind { get; set; } // 自風
//         public List<Tile> DoraIndicators { get; set; } = new List<Tile>(); // ドラ表示牌
//         public List<Tile> UraDoraIndicators { get; set; } = new List<Tile>(); // 裏ドラ表示牌 (リーチ時のみ)
//         public bool IsRiichi { get; set; } // リーチしているか
//         public bool IsIppatsu { get; set; } // 一発か
//         public bool IsMenzenTsumo { get; set; } // 面前でのツモ和了か
//         public bool IsRon { get; set; } // ロン和了か
//         public bool IsChankan { get; set; } // 槍槓か
//         public bool IsRinshanKaihou { get; set; } // 嶺上開花か
//         public int Honba { get; set; } // 本場
//         public Player WinningPlayer { get; set; } // 和了プレイヤー
//         public Wall GameWall { get; set; } // 壁牌 (ドラ計算などに使用)
//         // その他、天和・地和・人和の判定に必要な情報（第一ツモか、など）
//         public bool IsFirstTurn { get; set; }
//         public bool IsDealerFirstDiscard { get; set; } // 配牌時の最初の打牌か (人和用)

//         public GameContext(MahjongGameManager gameManager, Player winningPlayer, Tile winningTile, bool isRon)
//         {
//             RoundWind = gameManager.GetCurrentWind();
//             PlayerWind = gameManager.GetSeatWinds()[winningPlayer.SeatIndex];
//             DoraIndicators = gameManager.GetRemainingTileCount() > 0 ? gameManager.GetWall().DoraIndicator : new List<Tile>();
//             UraDoraIndicators = (winningPlayer.IsRiichi && gameManager.GetRemainingTileCount() > 0) ? gameManager.GetWall().HiddenDoraIndicator : new List<Tile>();
//             IsRiichi = winningPlayer.IsRiichi;
//             IsIppatsu = winningPlayer.IsIppatsu; // これはTurnManager等で適切に設定する必要がある
//             IsMenzenTsumo = !isRon && winningPlayer.Melds.Count == 0;
//             IsRon = isRon;
//             WinningPlayer = winningPlayer;
//             GameWall = gameManager.GetWall(); // MahjongGameManagerに GetWall() が必要

//             // TODO: 一発、槍槓、嶺上開花、天和、地和、人和などのフラグを適切に設定するロジックを MahjongGameManager や TurnManager に実装し、
//             //       GameContext に渡す必要があります。
//             // IsChankan = ...;
//             // IsRinshanKaihou = ...;
//             // IsFirstTurn = ...; (gameManager.TurnCount == 0 のような判定)
//             // IsDealerFirstDiscard = ...;
//         }
//     }


//     public static class YakuChecker
//     {
//         public static List<Yaku> CheckYaku(Player player, Tile winningTile, HandAnalysisResult analysis, GameContext context)
//         {
//             var yakuList = new List<Yaku>();
//             var allTiles = new List<Tile>(player.Hand);
//             if (!context.IsRon) // ツモ和了の場合、和了牌を手牌に加える (既に加えられている場合は不要)
//             {
//                 // Player.Draw() で既に手牌に加えられている想定
//             }
//             else // ロン和了の場合、和了牌を手牌に一時的に加えて判定 (既に加えた手牌で analysis が来ているなら不要)
//             {
//                 // allTiles.Add(winningTile);
//             }


//             // --- 役判定ロジック ---
//             // 各役の判定メソッドを呼び出し、成立していればyakuListに追加する

//             // 1翻役
//             CheckRiichi(player, context, yakuList);
//             CheckIppatsu(player, context, yakuList);
//             CheckMenzenTsumo(player, context, yakuList);
//             CheckTanyao(analysis, yakuList);
//             CheckPinfu(analysis, context, yakuList);
//             CheckYakuhai(analysis, context, yakuList);
//             CheckHaiteiRaoyue(player, context, yakuList); // 海底撈月 (ツモ)
//             CheckHouteiRaoyui(player, context, yakuList); // 河底撈魚 (ロン)
//             CheckRinshanKaihou(context, yakuList);
//             CheckChankan(context, yakuList);

//             // 2翻役
//             CheckDoubleRiichi(player, context, yakuList); // ダブルリーチ
//             CheckSanshokuDoujun(analysis, yakuList);      // 三色同順
//             CheckIkkitsuukan(analysis, yakuList);         // 一気通貫
//             CheckHonchantaiyao(analysis, yakuList);       //混全帯幺九
//             CheckChitoitsu(analysis, yakuList);           // 七対子
//             CheckToitoihou(analysis, yakuList);           // 対々和
//             CheckSanankou(analysis, yakuList);            // 三暗刻
//             CheckSankantsu(analysis, yakuList);           // 三槓子
//             CheckSanshokuDoukou(analysis, yakuList);      // 三色同刻
//             CheckShousangen(analysis, yakuList);          // 小三元
//             CheckHonroutou(analysis, yakuList);           // 混老頭

//             // 3翻役
//             CheckRyanpeikou(analysis, yakuList);          // 二盃口
//             CheckJunchanTaiyao(analysis, yakuList);       // 純全帯幺九
//             CheckHonitsu(analysis, yakuList);             // 混一色

//             // 6翻役
//             CheckChinitsu(analysis, yakuList);            // 清一色

//             // 役満
//             CheckKokushiMusou(analysis, winningTile, yakuList);    // 国士無双
//             CheckSuuankou(analysis, winningTile, yakuList);        // 四暗刻
//             CheckDaisangen(analysis, yakuList);           // 大三元
//             CheckShousuushii(analysis, context, yakuList); // 小四喜
//             CheckDaisuushii(analysis, context, yakuList);  // 大四喜
//             CheckTsuuiisou(analysis, yakuList);           // 字一色
//             CheckChinroutou(analysis, yakuList);          // 清老頭
//             CheckRyuuiisou(analysis, yakuList);           // 緑一色
//             CheckChuurenPoutou(analysis, winningTile, yakuList); // 九蓮宝燈
//             CheckSuukantsu(analysis, yakuList);           // 四槓子
//             CheckTenhou(context, yakuList);               // 天和
//             CheckChiihou(context, yakuList);              // 地和

//             // ドラ・裏ドラ・赤ドラは役ではないが、翻計算に影響
//             AddDora(player, context, yakuList);

//             // 役がない場合は和了できない (チョンボ)
//             // ただし、ドラのみでの和了は不可とするルールの場合、ここでチェックが必要。
//             // 一般的には、何かしらの役があればドラは有効。
//             if (!yakuList.Any(y => y.Name != "ドラ" && y.Name != "赤ドラ" && y.Name != "裏ドラ" && y.Name != "抜きドラ")) // "抜きドラ" は北抜きなど特殊ルール用
//             {
//                 // 役がない場合は空のリストを返すか、エラー処理を行う
//                 // ここでは、ScoreManager側で最終的に役があるか確認することを想定
//             }

//             // 食い下がりを考慮
//             if (!player.IsMenzen()) // 面前でない場合
//             {
//                 foreach (var yaku in yakuList)
//                 {
//                     if (yaku.Han != yaku.HanClosedOnly)
//                     {
//                         yaku.Han = yaku.HanClosedOnly;
//                     }
//                 }
//             }

//             return yakuList;
//         }

//         private static bool IsMenzen(Player player)
//         {
//             // 副露（鳴き）がない状態を面前とする
//             // 暗槓は面前扱いとなるが、明槓・加槓は非面前扱いとするか注意が必要
//             // 一般的に麻雀のルールでは、暗槓は面前扱いを継続する
//             return player.Melds.All(m => m.Type == MeldType.Ankan);
//         }


//         // --- 以下、各役の判定メソッドの雛形 ---

//         private static void CheckRiichi(Player player, GameContext context, List<Yaku> yakuList)
//         {
//             if (context.IsRiichi)
//             {
//                 yakuList.Add(new Yaku("立直 (リーチ)", 1));
//             }
//         }

//         private static void CheckIppatsu(Player player, GameContext context, List<Yaku> yakuList)
//         {
//             if (context.IsIppatsu && context.IsRiichi) // 一発はリーチが前提
//             {
//                 yakuList.Add(new Yaku("一発 (イッパツ)", 1));
//             }
//         }

//         private static void CheckMenzenTsumo(Player player, GameContext context, List<Yaku> yakuList)
//         {
//             if (context.IsMenzenTsumo)
//             {
//                 yakuList.Add(new Yaku("門前清自摸和 (メンゼンツモ)", 1));
//             }
//         }

//         // 牌の属性を判定するヘルパーメソッド (仮)
//         private static bool IsYaochuhai(Tile tile) // 幺九牌（ヤオチューハイ）か？
//         {
//             if (tile.Suit == Suit.Honor) return true;
//             return tile.Number == 1 || tile.Number == 9;
//         }

//         private static bool IsTerminal(Tile tile) // 老頭牌（ロートーハイ）か？ (1,9の数牌)
//         {
//             if (tile.Suit == Suit.Honor) return false;
//             return tile.Number == 1 || tile.Number == 9;
//         }

//         private static void CheckTanyao(HandAnalysisResult analysis, List<Yaku> yakuList)
//         {
//             // HandAnalysisResult に全ての牌が中張牌（チュンチャンパイ：2～8の数牌）かどうかのフラグを持たせるか、
//             // ここで analysis.GetAllTilesInHand() を使って判定する。
//             bool isTanyao = analysis.GetAllTilesInHand().All(tile => !IsYaochuhai(tile));
//             if (isTanyao)
//             {
//                 yakuList.Add(new Yaku("断幺九 (タンヤオ)", 1));
//             }
//         }


//         private static void CheckPinfu(HandAnalysisResult analysis, GameContext context, List<Yaku> yakuList)
//         {
//             // 平和 (ピンフ) の条件:
//             // 1. 面前であること (GameContext.IsMenzenTsumo または GameContext.IsRon で、かつ鳴きがない)
//             // 2. 4面子がすべて順子であること (HandAnalysisResult.MentsuList で判定)
//             // 3. 雀頭が役牌（場風・自風・三元牌）でないこと (HandAnalysisResult.Jantou で判定)
//             // 4. 待ちが両面待ちであること (HandAnalysisResult.WinningPatter が Ryanmen であること)
//             //    ※ HandAnalysisResult に待ちの形に関する情報が必要
//             if (context.WinningPlayer.IsMenzen() &&
//                 analysis.MentsuList.All(m => m.Type == MeldType.Chi) && // 全て順子
//                 analysis.MentsuList.Count == 4 && // 4面子ある
//                 analysis.Jantou != null &&
//                 !IsYakuhaiTile(analysis.Jantou, context.RoundWind, context.PlayerWind) && // 雀頭が役牌でない
//                 analysis.IsRyanmenMachi) // 両面待ち (HandAnalysisResultにこの情報が必要)
//             {
//                 // ツモ和了の場合、符がないのでピンフツモとして成立
//                 // ロン和了の場合も同様に成立
//                 // (符計算はScoreManagerで行うが、ピンフの成立条件としてここで判定)
//                 yakuList.Add(new Yaku("平和 (ピンフ)", 1));
//             }
//         }

//         private static bool IsYakuhaiTile(Tile tile, Wind roundWind, Wind playerWind)
//         {
//             if (tile.Suit == Suit.Honor)
//             {
//                 if (tile.Number >= 5) return true; // 三元牌 (白・發・中)
//                 if ((int)roundWind == tile.Number - 1) return true; // 場風
//                 if ((int)playerWind == tile.Number - 1) return true; // 自風
//             }
//             return false;
//         }


//         private static void CheckYakuhai(HandAnalysisResult analysis, GameContext context, List<Yaku> yakuList)
//         {
//             int yakuhaiCount = 0;
//             // HandAnalysisResult.MentsuList から刻子（コーツ）または槓子（カンツ）を取得し、
//             // それが役牌（場風、自風、三元牌）かどうかを判定する。
//             foreach (var mentsu in analysis.MentsuList)
//             {
//                 if (mentsu.Type == MeldType.Pon || mentsu.Type == MeldType.Ankan || mentsu.Type == MeldType.Minkan || mentsu.Type == MeldType.Kakan) // 刻子・槓子
//                 {
//                     Tile representativeTile = mentsu.Tiles[0]; // 刻子・槓子の代表牌
//                     if (representativeTile.Suit == Suit.Honor)
//                     {
//                         if (representativeTile.Number == 5) // 白
//                         {
//                             yakuList.Add(new Yaku("役牌 白 (ハク)", 1));
//                         }
//                         else if (representativeTile.Number == 6) // 發
//                         {
//                             yakuList.Add(new Yaku("役牌 發 (ハツ)", 1));
//                         }
//                         else if (representativeTile.Number == 7) // 中
//                         {
//                             yakuList.Add(new Yaku("役牌 中 (チュン)", 1));
//                         }
//                         else if (representativeTile.Number - 1 == (int)context.RoundWind) // 場風
//                         {
//                             yakuList.Add(new Yaku($"役牌 場風 {context.RoundWind.ToString()}", 1));
//                         }
//                         else if (representativeTile.Number - 1 == (int)context.PlayerWind) // 自風
//                         {
//                             yakuList.Add(new Yaku($"役牌 自風 {context.PlayerWind.ToString()}", 1));
//                         }
//                         // 連風牌（ダブ東、ダブ南など）は、場風と自風が同じ場合に2翻とするか、
//                         // それぞれ1翻として計2翻とするかはルールによる。ここでは各1翻として重複して追加される形。
//                         // ScoreManager側で翻数を合計する際に調整するか、Yakuクラスに複合役としての情報を追加する。
//                         // 一般的には、場風の役牌と自風の役牌がそれぞれ成立し、結果的に2翻になる。
//                     }
//                 }
//             }
//         }

//         private static void CheckHaiteiRaoyue(Player player, GameContext context, List<Yaku> yakuList)
//         {
//             // 海底撈月 (ハイテイラオユエ): 壁牌の最後の牌でツモ和了
//             if (!context.IsRon && context.GameWall.Count == 0) // ツモ和了 かつ 壁牌が0枚
//             {
//                 yakuList.Add(new Yaku("海底撈月 (ハイテイラオユエ)", 1));
//             }
//         }

//         private static void CheckHouteiRaoyui(Player player, GameContext context, List<Yaku> yakuList)
//         {
//             // 河底撈魚 (ホウテイラオユイ): 他家の最後の捨て牌でロン和了
//             // TurnManagerで最後の捨て牌であるというフラグを管理し、context経由で渡す必要がある。
//             //  if (context.IsRon && context.IsLastDiscardInRound)
//             //  {
//             //      yakuList.Add(new Yaku("河底撈魚 (ホウテイラオユイ)", 1));
//             //  }
//             //  現時点では IsLastDiscardInRound の情報がないためコメントアウト
//         }


//         private static void CheckRinshanKaihou(GameContext context, List<Yaku> yakuList)
//         {
//             if (context.IsRinshanKaihou)
//             {
//                 yakuList.Add(new Yaku("嶺上開花 (リンシャンカイホウ)", 1));
//             }
//         }

//         private static void CheckChankan(GameContext context, List<Yaku> yakuList)
//         {
//             if (context.IsChankan)
//             {
//                 yakuList.Add(new Yaku("槍槓 (チャンカン)", 1));
//             }
//         }


//         // --- ドラ関連 ---
//         private static void AddDora(Player player, GameContext context, List<Yaku> yakuList)
//         {
//             var handAndMelds = player.GetAllTilesInHandAndMelds(); // 手牌と副露牌（暗槓含む）の全て

//             // 表ドラ
//             foreach (var doraIndicator in context.DoraIndicators)
//             {
//                 Tile doraTile = CalculateActualDoraTile(doraIndicator, context.GameWall);
//                 int doraCount = handAndMelds.Count(tile => tile.EqualsForDora(doraTile)); // TileクラスにDora判定用のEqualsメソッドが必要
//                 if (doraCount > 0)
//                 {
//                     yakuList.Add(new Yaku("ドラ", doraCount));
//                 }
//             }

//             // 裏ドラ (リーチ時のみ)
//             if (context.IsRiichi && !context.IsRon) // リーチかつツモ和了の場合、またはリーチロンで裏ドラありルールの場合
//             {
//                 // 一般的にロン和了でも裏ドラは乗る
//                 foreach (var uraDoraIndicator in context.UraDoraIndicators)
//                 {
//                     Tile uraDoraTile = CalculateActualDoraTile(uraDoraIndicator, context.GameWall);
//                     int uraDoraCount = handAndMelds.Count(tile => tile.EqualsForDora(uraDoraTile));
//                     if (uraDoraCount > 0)
//                     {
//                         yakuList.Add(new Yaku("裏ドラ", uraDoraCount));
//                     }
//                 }
//             }


//             // 赤ドラ (TileクラスにIsRedフラグがある前提)
//             int redDoraCount = handAndMelds.Count(tile => tile.IsRed);
//             if (redDoraCount > 0)
//             {
//                 yakuList.Add(new Yaku("赤ドラ", redDoraCount));
//             }
//         }

//         private static Tile CalculateActualDoraTile(Tile indicatorTile, Wall wall)
//         {
//             // Wallクラスにドラ牌を計算するロジックを実装する (Wall.csのCalculateNextTileが該当)
//             // ここでは仮にWallクラスのメソッドを呼び出す形とする
//             return wall.CalculateNextTileForDora(indicatorTile);
//         }


//         // 他の役の判定メソッドも同様に定義していく...
//         // 例：CheckDoubleRiichi, CheckSanshokuDoujun, CheckKokushiMusou など

//         // (以下、他の役の判定メソッドの雛形が続く)
//         private static void CheckDoubleRiichi(Player player, GameContext context, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckSanshokuDoujun(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckIkkitsuukan(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckHonchantaiyao(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckChitoitsu(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckToitoihou(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckSanankou(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckSankantsu(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckSanshokuDoukou(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckShousangen(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckHonroutou(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckRyanpeikou(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckJunchanTaiyao(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckHonitsu(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckChinitsu(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckKokushiMusou(HandAnalysisResult analysis, Tile winningTile, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckSuuankou(HandAnalysisResult analysis, Tile winningTile, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckDaisangen(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckShousuushii(HandAnalysisResult analysis, GameContext context, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckDaisuushii(HandAnalysisResult analysis, GameContext context, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckTsuuiisou(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckChinroutou(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckRyuuiisou(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckChuurenPoutou(HandAnalysisResult analysis, Tile winningTile, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckSuukantsu(HandAnalysisResult analysis, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckTenhou(GameContext context, List<Yaku> yakuList) { /* ... */ }
//         private static void CheckChiihou(GameContext context, List<Yaku> yakuList) { /* ... */ }

//     }
// }