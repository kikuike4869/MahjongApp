// using System.Security.Cryptography.X509Certificates;

// namespace MahjongApp
// {
//     public enum WinningFormType
//     {
//         None,
//         Standard,
//         Chiitoitsu,
//         KokushiMusou
//     }

//     public class HandAnalysisResult
//     {
//         public List<Meld> MentsuList { get; private set; } = new List<Meld>();
//         public Tile? Jantou { get; private set; }
//         public WinningFormType FormType { get; private set; } = WinningFormType.None;
//         public bool IsWinningHand { get; private set; } = false;
//         public bool IsMenzen { get; private set; }
//         public List<Tile> AnalyzedHand { get; private set; } = new List<Tile>();
//         public Tile? WinningTile { get; private set; }
//         public bool IsTsumo { get; private set; }
//         public bool IsRyanmenMachi { get; private set; } = false;

//         public List<PossibleHandPattern> AllPossiblePatterns { get; private set; } = new List<PossibleHandPattern>();

//         private Player _player;

//         // 全パターン解析用の新しいメソッド (またはコンストラクタ)
//         // 例: 手牌のみを渡して、全ての待ちやシャンテン数を計算する       
//         public HandAnalysisResult(Player player, Tile winningTile, bool isTsumo)
//         {
//             WinningTile = winningTile;
//             IsTsumo = isTsumo;
//             IsMenzen = player.IsMenzen();
//             AnalyzedHand = SortTiles(new List<Tile>(player.Hand));

//             if (!isTsumo)
//             {
//                 AnalyzedHand.Add(winningTile);
//             }

//             // 副露牌を初期の面子として設定
//             var initialMelds = new List<Meld>(player.Melds);

//             // 1. 全ての可能な和了形を探索する
//             //    - 通常形 (4面子1雀頭) の全ての分解パターン
//             //    - 七対子
//             //    - 国士無双 (13面待ちも考慮)
//             FindAllPossibleStandardForms(AnalyzedHand, initialMelds);
//             CheckAndAddChiitoitsuPattern(AnalyzedHand);
//             CheckAndAddKokushiMusouPattern(AnalyzedHand);

//         }

//         /// <summary>
//         /// 手牌の全ての可能な通常形（4面子1雀頭）の組み合わせを探索し、AllPossiblePatternsに追加します。
//         /// 副露も考慮します。
//         /// </summary>
//         /// <param name="handTiles">解析対象の手牌（ソート済み）。副露牌は含まない。</param>
//         /// <param name="existingMelds">既に確定している副露のリスト。</param>
//         private void FindAllPossibleStandardForms(List<Tile> handTiles, List<Meld> existingMelds)
//         {
//             // バックトラッキングや再帰を使って全ての組み合わせを試す
//             // 必要な面子数
//             int requiredHandMelds = 4 - existingMelds.Count;
//             if (requiredHandMelds < 0) requiredHandMelds = 0;

//             List<PossibleHandPattern> foundPatterns = new List<PossibleHandPattern>();
//             FindAllStandardHandsRecursive(
//                 new List<Tile>(handTiles), // 解析する手牌 (副露牌は除く)
//                 requiredHandMelds,
//                 new List<Meld>(),          // 現在構築中の手牌からの面子
//                 null,                      // 現在構築中の雀頭
//                 existingMelds,             // 既に確定している副露
//                 foundPatterns
//             );

//             AllPossiblePatterns.AddRange(foundPatterns);
//         }

//         /// <summary>
//         /// 再帰的に通常手（4面子1雀頭）の全ての組み合わせを探す。
//         /// </summary>
//         /// <param name="remainingHand">処理対象の残り手牌（ソート済み）。</param>
//         /// <param name="targetMelds">あと何個の面子を手牌から作る必要があるか。</param>
//         /// <param name="currentMeldsFromHand">現在の手牌から構築された面子のリスト。</param>
//         /// <param name="currentJantou">現在の雀頭候補。</param>
//         /// <param name="fixedMelds">既に確定している副露のリスト。</param>
//         /// <param name="foundPatterns">見つかった全ての有効なパターンを格納するリスト。</param>
//         private void FindAllStandardHandsRecursive(
//             List<Tile> remainingHand,
//             int targetMelds,
//             List<Meld> currentMeldsFromHand,
//             Tile? currentJantou,
//             List<Meld> fixedMelds,
//             List<PossibleHandPattern> foundPatterns)
//         {
//             // ベースケース: 必要な面子数が0になった場合
//             if (targetMelds == 0)
//             {
//                 // 雀頭があり，手牌が空ならば有効なパターン
//                 if (currentJantou != null && remainingHand.Count == 0)
//                 {
//                     var completeMelds = new List<Meld>(fixedMelds);
//                     completeMelds.AddRange(currentMeldsFromHand);
//                     foundPatterns.Add(new PossibleHandPattern(WinningFormType.Standard, completeMelds, currentJantou, 0));
//                 }
//                 // 雀頭がまだ見つかっていない場合、残りの手牌が対子ならそれが雀頭
//                 else if (currentJantou == null && remainingHand.Count == 2 && TilesAreEqual(remainingHand[0], remainingHand[1]))
//                 {
//                     var completeMelds = new List<Meld>(fixedMelds);
//                     completeMelds.AddRange(currentMeldsFromHand);
//                     foundPatterns.Add(new PossibleHandPattern(WinningFormType.Standard, completeMelds, remainingHand[0], 0));
//                 }

//                 return;
//             }

//             if (remainingHand.Count < 3 * targetMelds + (currentJantou == null ? 2 : 0))// 雀頭がなければさらに2枚必要
//             {
//                 // ここでシャンテン数を計算して記録することも可能
//                 // int shanten = CalculateShantenForPartialHand(remainingHand, targetMelds, currentJantou != null);
//                 // foundPatterns.Add(new PossibleHandPattern(WinningFormType.None, new List<Meld>(fixedMelds).Concat(currentMeldsFromHand).ToList(), currentJantou, shanten));
//                 return;
//             }

//             // パターン1: 雀頭を先に探す (まだ雀頭がない場合)
//             if (currentJantou == null && remainingHand.Count >= 2)
//             {
//                 for (int i = 0; i < remainingHand.Count - 1; i++)
//                 {
//                     if (TilesAreEqual(remainingHand[i], remainingHand[i + 1]))
//                     {
//                         Tile potentialJantou = remainingHand[i];
//                         List<Tile> handAfterJantou = new List<Tile>(remainingHand);
//                         handAfterJantou.RemoveAt(i + 1); // 後ろの要素から削除
//                         handAfterJantou.RemoveAt(i);   // 前の要素を削除

//                         FindAllStandardHandsRecursive(handAfterJantou, targetMelds, new List<Meld>(currentMeldsFromHand), potentialJantou, fixedMelds, foundPatterns);
//                     }
//                 }
//             }

//             // パターン2: 面子を探す
//             if (remainingHand.Count >= 3)
//             {
//                 // 刻子を探す
//                 if (TilesAreEqual(remainingHand[0], remainingHand[1], remainingHand[2]))
//                 {
//                     Meld koutsu = new Meld { Type = MeldType.Koutsu, Tiles = new List<Tile> { remainingHand[0], remainingHand[1], remainingHand[2] }, FromPlayerIndex = -1, IsOpen = false };
//                     List<Meld> nextMeldsFromHand = new List<Meld>(currentMeldsFromHand);
//                     nextMeldsFromHand.Add(koutsu);
//                     List<Tile> handAfterKoutsu = remainingHand.Skip(3).ToList();
//                     FindAllStandardHandsRecursive(handAfterKoutsu, targetMelds - 1, nextMeldsFromHand, currentJantou, fixedMelds, foundPatterns);
//                 }

//                 // 順子を探す
//                 if (remainingHand[0].Suit != Suit.Honor)
//                 {
//                     Tile tile1 = remainingHand[0];
//                     // tile2, tile3 を手牌から効率的に見つける
//                     // 単純な FirstOrDefault はソートされていても効率が悪い場合がある。
//                     // Count や IndexOf をうまく使うか、牌の種類ごとにカウントしておくなどの前処理が有効。
//                     var availableTiles = new Dictionary<string, int>();
//                     foreach (var t in remainingHand)
//                     {
//                         string key = $"{t.Suit}_{t.Number}";
//                         if (!availableTiles.ContainsKey(key)) availableTiles[key] = 0;
//                         availableTiles[key]++;
//                     }

//                     string t1Key = $"{tile1.Suit}_{tile1.Number}";
//                     string t2Key = $"{tile1.Suit}_{tile1.Number + 1}";
//                     string t3Key = $"{tile1.Suit}_{tile1.Number + 2}";
//                     if (availableTiles.ContainsKey(t1Key) && availableTiles[t1Key] > 0 &&
//                         availableTiles.ContainsKey(t2Key) && availableTiles[t2Key] > 0 &&
//                         availableTiles.ContainsKey(t3Key) && availableTiles[t3Key] > 0)
//                     {
//                         // 実際に牌のインスタンスを見つけて抜き出す必要がある
//                         Tile actualT1 = remainingHand.First(t => t.Suit == tile1.Suit && t.Number == tile1.Number);
//                         Tile actualT2 = remainingHand.First(t => t.Suit == tile1.Suit && t.Number == tile1.Number + 1);
//                         Tile actualT3 = remainingHand.First(t => t.Suit == tile1.Suit && t.Number == tile1.Number + 2);

//                         Meld shuntsu = new Meld { Type = MeldType.Shuntsu, Tiles = new List<Tile> { actualT1, actualT2, actualT3 }, FromPlayerIndex = -1, IsOpen = false };
//                         List<Meld> nextMeldsFromHand = new List<Meld>(currentMeldsFromHand);
//                         nextMeldsFromHand.Add(shuntsu);

//                         List<Tile> handAfterShuntsu = new List<Tile>(remainingHand);
//                         handAfterShuntsu.Remove(actualT1); // Remove の挙動に注意 (インスタンス or 値) TileクラスのEquals/GetHashCodeの実装が重要
//                         handAfterShuntsu.Remove(actualT2);
//                         handAfterShuntsu.Remove(actualT3);
//                         handAfterShuntsu = SortTiles(handAfterShuntsu); // Remove後、再度ソートした方が安全

//                         FindAllStandardHandsRecursive(handAfterShuntsu, targetMelds - 1, nextMeldsFromHand, currentJantou, fixedMelds, foundPatterns);
//                     }
//                 }
//             }
//         }

//         /// <summary>
//         /// 七対子パターンをチェックし、AllPossiblePatternsに追加します。
//         /// </summary>
//         private void CheckAndAddChiitoitsuPattern(List<Tile> hand)
//         {
//             if (IsMenzen && hand.Count == 14) // 七対子は門前限定
//             {
//                 var groups = hand.GroupBy(t => $"{t.Suit}_{t.Number}").ToList();
//                 if (groups.Count == 7 && groups.All(g => g.Count() == 2))
//                 {
//                     // 七対子の面子表現は特殊なので、Meldリストは空か、あるいは特別なMeldTypeで表現する
//                     List<Meld> chiitoitsuMelds = new List<Meld>() { new Meld() { Type = MeldType.ChiitoitsuMelds, Tiles = hand, IsOpen = false } };
//                     // AllPossiblePatterns.Add(new PossibleHandPattern(WinningFormType.Chiitoitsu, chiitoitsuMelds, null, CalculateShantenForChiitoitsu(hand)));
//                     AllPossiblePatterns.Add(new PossibleHandPattern(WinningFormType.Chiitoitsu, chiitoitsuMelds, null, 99));
//                 }
//             }
//         }

//         /// <summary>
//         /// 国士無双パターンをチェックし、AllPossiblePatternsに追加します。
//         /// </summary>
//         private void CheckAndAddKokushiMusouPattern(List<Tile> hand)
//         {
//             if (IsMenzen) // 国士無双は門前限定
//             {
//                 // (CheckKokushiMusou と同様のロジックで判定)
//                 // 13面待ちの場合と単騎待ちの場合でシャンテン数が異なる
//                 // ... 簡略化のため省略 ...
//                 if (CheckKokushiMusou(hand))
//                 {
//                     List<Meld> kokushiMelds = new List<Meld>() { new Meld() { Type = MeldType.KokushiMelds, Tiles = hand, IsOpen = false } };
//                     AllPossiblePatterns.Add(new PossibleHandPattern(WinningFormType.KokushiMusou, kokushiMelds, null, 99));
//                     // AllPossiblePatterns.Add(new PossibleHandPattern(WinningFormType.KokushiMusou, kokushiMelds, null, CalculateShantenForKokushiMusou(hand)));
//                 }
//             }
//         }


//         private List<Tile> SortTiles(List<Tile> cuurentHand) { return cuurentHand.OrderBy(t => t.Suit).ThenBy(t => t.Number).ToList(); }

//         private bool CheckKokushiMusou(List<Tile> hand)
//         {
//             if (hand.Count != 14) return false;

//             var yaochuhaiTyples = new List<string>{
//                 "Manzu_1", "Manzu_9",
//                 "Pinzu_1", "Pinzu_9",
//                 "Souzu_1", "Souzu_9",
//                 "Honor_1", "Honor_2", "Honor_3",
//                 "Honor_4", "Honor_5", "Honor_6","Honor_7"
//             };

//             var handTileStrings = hand.Select(t => $"{t.Suit}_{t.Number}").ToList();
//             var distinctHandTiles = handTileStrings.Distinct().ToList();

//             if (distinctHandTiles.Count != 13) return false;

//             var groups = handTileStrings.GroupBy(s => s).ToList();
//             if (groups.Count(g => g.Count() == 2) == 1 && groups.Count(g => g.Count() == 1) == 12)
//             {
//                 return true;
//             }

//             return false;
//         }

//         private bool TilesAreEqual(Tile tile1, Tile tile2)
//         {
//             if (tile1 == null || tile2 == null) return false;
//             return tile1.Suit == tile2.Suit && tile1.Number == tile2.Number;
//         }
//         private bool TilesAreEqual(Tile tile1, Tile tile2, Tile tile3)
//         {
//             if (tile1 == null || tile2 == null || tile3 == null) return false;
//             return tile1.Suit == tile2.Suit && tile1.Suit == tile3.Suit &&
//                    tile1.Number == tile2.Number && tile1.Number == tile3.Number;
//         }

//         // CheckRyanmenMachi は、特定の和了牌に対して、それが両面待ちだったかを判定する
//         // 全パターン解析の場合は、各パターンがどのような待ちになるかを別途計算する必要がある
//         private void CheckRyanmenMachi(List<Meld> melds, Tile? jantou, Tile? winningTile, bool isTsumo)
//         {
//             if (winningTile == null)
//             {
//                 IsRyanmenMachi = false;
//                 return;
//             }

//             // この判定は、和了形が確定した後に、その和了牌が両面待ちの一部だったかを見るもの。
//             // 例えば、手牌に 123m 45p があり、6p で和了した場合、456p が順子となり、その6pが両面待ち。
//             // 和了牌を含む順子を探す
//             foreach (var meld in melds)
//             {
//                 if (meld.Type == MeldType.Shuntsu && meld.Tiles.Contains(winningTile))
//                 {
//                     // winningTile が順子の端の牌 (最小数字または最大数字) であれば両面待ちの可能性
//                     var sortedTiles = meld.Tiles.OrderBy(t => t.Number).ToList();
//                     if (TilesAreEqual(sortedTiles[0], winningTile) && winningTile.Number < 7) // 123 の 1 や 789 の 7 など (ペンチャン張りの場合も考慮が必要)
//                     {
//                         // この順子が x (x+1) (x+2=winningTile) または (winningTile=x) (x+1) (x+2) の形
//                         // 例えば、234 の 4 で和了 (待ち2,5)、または 456 の 4 で和了 (待ち3,6)
//                         // winningTile が順子の真ん中の牌ではない場合、両面待ちの可能性がある。
//                         // より正確には、和了牌を除いた残りの2枚が連続しているか。
//                         List<Tile> remainingInShuntsu = new List<Tile>(meld.Tiles);
//                         remainingInShuntsu.Remove(winningTile); // ここもインスタンス削除
//                         if (remainingInShuntsu.Count == 2)
//                         {
//                             var rSorted = remainingInShuntsu.OrderBy(t => t.Number).ToList();
//                             if (rSorted[0].Number + 1 == rSorted[1].Number) // 残りが連番
//                             {
//                                 // かつ、winningTileがその連番のどちらかの隣である
//                                 if (winningTile.Number == rSorted[0].Number - 1 || winningTile.Number == rSorted[1].Number + 1)
//                                 {
//                                     IsRyanmenMachi = true;
//                                     return;
//                                 }
//                             }
//                         }
//                     }
//                     else if (TilesAreEqual(sortedTiles[2], winningTile) && winningTile.Number > 2)
//                     {
//                         List<Tile> remainingInShuntsu = new List<Tile>(meld.Tiles);
//                         remainingInShuntsu.Remove(winningTile);
//                         if (remainingInShuntsu.Count == 2)
//                         {
//                             var rSorted = remainingInShuntsu.OrderBy(t => t.Number).ToList();
//                             if (rSorted[0].Number + 1 == rSorted[1].Number)
//                             {
//                                 if (winningTile.Number == rSorted[0].Number - 1 || winningTile.Number == rSorted[1].Number + 1)
//                                 {
//                                     IsRyanmenMachi = true;
//                                     return;
//                                 }
//                             }
//                         }
//                     }
//                 }
//             }
//             IsRyanmenMachi = false;
//         }

//         // シャンテン数を計算するメソッド (非常に複雑なので、ここではプレースホルダー)
//         private int CalculateShanten(List<Tile> hand, List<Meld> melds, Tile? jantou)
//         {
//             // TODO: シャンテン数計算ロジックの実装
//             // 一般形: 8 - (面子数 * 2) - (塔子数) - (雀頭数)
//             // 特殊形: 国士無双、七対子のシャンテン数も別途計算
//             return 99; // 仮の値
//         }
//         private int CalculateShantenForChiitoitsu(List<Tile> hand)
//         {
//             if (hand.Count > 14) return 99; //ありえない
//             // 対子の数を数える
//             var groups = hand.GroupBy(t => $"{t.Suit}_{t.Number}");
//             int pairCount = groups.Count(g => g.Count() >= 2);
//             int distinctTiles = groups.Count();

//             // 七対子のシャンテン数 = 6 - 対子の数 + (7 - 牌の種類数)  (ただし牌の種類数が7未満の場合)
//             // または、必要な対子の数 = 7 - pairCount
//             // 不足している牌の数 = (7 - pairCount) + (pairCount - distinctTilesWhereCountIsOne)
//             int shanten = 6 - pairCount;
//             if (distinctTiles < 7 && pairCount < 7)
//             { // 7種類に足りていない場合、その分も考慮
//                 shanten += (7 - distinctTiles);
//             }
//             // より一般的な計算: 6 - (対子の数) + max(0, 7 - (対子の数 + 単独牌の種類数))
//             int singleTileKindCount = groups.Count(g => g.Count() == 1);
//             // shanten = 6 - pairCount + Math.Max(0, 7 - (pairCount + singleTileKindCount));

//             // 例：1122334 の場合、対子3つ、単独牌1つ => 6 - 3 = 3向聴 (実際は1向聴のはず)
//             // 対子をn個作るときのシャンテン数は 6-n
//             // 対子がk個、それ以外の牌がm種類ある場合、6-k + max(0, 7-(k+m))
//             //  var tileCounts = hand.GroupBy(tile => tile.ToString()).ToDictionary(g => g.Key, g => g.Count());
//             //  int pairs = tileCounts.Values.Count(c => c >= 2);
//             //  int kinds = tileCounts.Keys.Count;
//             //  if (kinds < 7) return 6 - pairs + (7 - kinds);
//             //  return 6 - pairs;
//             return shanten < 0 ? 0 : shanten;
//         }
//     }

//     public class PossibleHandPattern
//     {
//         public WinningFormType FormType { get; }
//         public List<Meld> Melds { get; } = new List<Meld>();
//         public Tile? Jantou { get; }
//         public int Shanten { get; }
//         public List<Tile> WaitingTiles { get; }

//         public PossibleHandPattern(WinningFormType formType, List<Meld> melds, Tile? jantou, int shanten)
//         {
//             FormType = formType;
//             Melds = melds;
//             Jantou = jantou;
//             Shanten = shanten;
//             WaitingTiles = CaluculateWaitingTiles();
//         }

//         List<Tile> CaluculateWaitingTiles()
//         {
//             // 待ち牌の計算ロジックを実装
//             // ここではダミーの待ち牌リストを返す
//             List<Tile> waitingTiles = new List<Tile>();
//             // 例: waitingTiles.Add(new Tile(Suit.Manzu, 1));
//             return waitingTiles;
//         }
//     }

//     public class PartialHandState
//     {
//         public int CompletedMelds;
//         public int TaatsuCount;
//         public bool HasJantou;
//         public int FloatingTiles;

//         // public int CalculateShanten()
//         // {
//         //     int shanten = 8 - CompletedMelds * 2 - TaatsuCount - (HasJantou ? 1 : 0);

//         //     int neededBlocks = 4 - CompletedMelds;
//         //     int neededTiledForBlocks = neededBlocks * 2;
//         // }

//         public int CalculateChiitoitsuShanten()
//         {
//             // 七対子のシャンテン数を計算するロジック
//             // 対子の数を数える
//             int pairCount = TaatsuCount;
//             int distinctTiles = FloatingTiles;

//             // 七対子のシャンテン数 = 6 - 対子の数 + (7 - 牌の種類数)  (ただし牌の種類数が7未満の場合)
//             // または、必要な対子の数 = 7 - pairCount
//             // 不足している牌の数 = (7 - pairCount) + (pairCount - distinctTilesWhereCountIsOne)
//             int shanten = 6 - pairCount;
//             if (distinctTiles < 7 && pairCount < 7)
//             { // 7種類に足りていない場合、その分も考慮
//                 shanten += (7 - distinctTiles);
//             }
//             return shanten < 0 ? 0 : shanten;
//         }

//         // public int CalculateKokushiMusouShanten()
//         // {
//         //     // 国士無双のシャンテン数を計算するロジック
//         //     int shanten = 13 -

//         // }
//     }
// }