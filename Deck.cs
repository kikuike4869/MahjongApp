using System; // Added for Random
using System.Collections.Generic; // Added for Queue and List
using System.Linq; // Added for Shuffle temporary list conversion

namespace MahjongApp
{
    class Deck
    {
        private Queue<Tile> Tiles;
        public Queue<Tile> WanPie { get; private set; }

        public List<Tile> Dora { get; private set; }
        public List<Tile> HiddenDora { get; private set; }
        public List<Tile> DoraIndicator { get; private set; }
        public List<Tile> HiddenDoraIndicator { get; private set; }

        public int Count => Tiles?.Count ?? 0;

        public Deck()
        {
            // QueueとListを初期化
            Tiles = new Queue<Tile>();
            Dora = new List<Tile>();
            HiddenDora = new List<Tile>();
            DoraIndicator = new List<Tile>();
            HiddenDoraIndicator = new List<Tile>();
            WanPie = new Queue<Tile>(); // Changed to Queue

            InitializeDeck();
        }

        public void InitializeDeck()
        {
            Tiles.Clear();
            Dora.Clear();
            HiddenDora.Clear();
            DoraIndicator.Clear();
            HiddenDoraIndicator.Clear();
            WanPie.Clear();

            // 一時リストで牌を生成・シャッフル
            List<Tile> generatedTiles = GenerateTiles();
            Shuffle(generatedTiles);

            // シャッフルされたリストからQueueに牌を移動
            foreach (var tile in generatedTiles)
            {
                Tiles.Enqueue(tile);
            }

            // ドラ、裏ドラ、王牌を設定 (Drawメソッド経由でQueueから取得)
            SetDoraIndicatorAndDora();
            SetHiddenDoraIndicatorAndHiddenDora();
            SetWanPie();
        }

        // GenerateTilesは一時リストを返すように変更
        private List<Tile> GenerateTiles()
        {
            var tempTiles = new List<Tile>(136);
            Suit[] suits = { Suit.Manzu, Suit.Pinzu, Suit.Souzu };
            foreach (var suit in suits)
            {
                for (int number = 1; number <= 9; number++)
                {
                    // 赤５を除いた通常牌を追加
                    int copies_num = (number == 5) ? 3 : 4;
                    for (int index = 0; index < copies_num; index++)
                    {
                        tempTiles.Add(new Tile(suit, number, index));
                    }
                }
            }

            for (int number = 1; number <= 7; number++)
            {
                for (int index = 0; index < 4; index++)
                {
                    tempTiles.Add(new Tile(Suit.Honor, number, index));
                }
            }

            // 赤５を追加
            tempTiles.Add(new Tile(Suit.Manzu, 5, 3, true));
            tempTiles.Add(new Tile(Suit.Pinzu, 5, 3, true));
            tempTiles.Add(new Tile(Suit.Souzu, 5, 3, true));

            return tempTiles;
        }

        // ShuffleはList<Tile>を受け取るように変更
        public void Shuffle(List<Tile> tilesToShuffle)
        {
            // Use shared random instance
            Random rng = SharedRandom.Instance;
            int n = tilesToShuffle.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (tilesToShuffle[k], tilesToShuffle[n]) = (tilesToShuffle[n], tilesToShuffle[k]);
            }
        }

        // DrawメソッドはQueueからDequeueするように変更
        public Tile Draw()
        {
            if (Tiles.Count == 0)
            {
                // Consider game state for draw from wanpai (kan draw) or end of game
                throw new InvalidOperationException("No Tiles left in the main deck.");
            }
            return Tiles.Dequeue();
        }

        // DrawFromWanPieもQueueからDequeueするように変更
        public Tile DrawFromWanPie()
        {
            if (WanPie.Count == 0)
            {
                // This case might happen during multiple kans, handle game logic appropriately
                throw new InvalidOperationException("No Tiles left in the wanpai.");
            }
            return WanPie.Dequeue();
        }


        // ドラ設定 (表示牌取得 + ドラ計算)
        private void SetDoraIndicatorAndDora()
        {
            // Draw 5 tiles for the Dora indicators from the main deck queue
            for (int i = 0; i < 5; i++)
            {
                 if (Tiles.Count > 0) // Ensure deck is not empty
                     DoraIndicator.Add(Draw());
                 else
                     throw new InvalidOperationException("Not enough tiles to set Dora indicators.");
            }
            // Calculate the first Dora based on the first indicator
            if (DoraIndicator.Count > 0)
                Dora.Add(CalculateNextTile(DoraIndicator[0]));
        }

        // 裏ドラ設定 (表示牌取得 + 裏ドラ計算)
        private void SetHiddenDoraIndicatorAndHiddenDora()
        {
             // Draw 5 tiles for the Hidden Dora indicators
            for (int i = 0; i < 5; i++)
            {
                 if (Tiles.Count > 0)
                     HiddenDoraIndicator.Add(Draw());
                 else
                     throw new InvalidOperationException("Not enough tiles to set Hidden Dora indicators.");
            }
             // Calculate the first Hidden Dora
            if (HiddenDoraIndicator.Count > 0)
                 HiddenDora.Add(CalculateNextTile(HiddenDoraIndicator[0]));
        }

        // 王牌設定 (嶺上牌4枚を取得)
        private void SetWanPie()
        {
            // Draw 4 tiles for the dead wall (嶺上牌)
            for (int i = 0; i < 4; i++)
            {
                 if (Tiles.Count > 0)
                     WanPie.Enqueue(Draw());
                 else
                     throw new InvalidOperationException("Not enough tiles to set WanPie.");
            }
        }


        /// <summary>
        /// ドラ表示牌から実際のドラ牌を計算します。
        /// </summary>
        private Tile CalculateNextTile(Tile indicatorTile)
        {
            int nextTileNumber;
            Suit nextTileSuit = indicatorTile.Suit;

            if (indicatorTile.Suit == Suit.Honor) // 字牌
            {
                // 風牌 (East=1 ... North=4) -> rolls over 1 -> 2 -> 3 -> 4 -> 1
                if (indicatorTile.Number >= 1 && indicatorTile.Number <= 4)
                {
                    nextTileNumber = (indicatorTile.Number == 4) ? 1 : indicatorTile.Number + 1;
                }
                // 三元牌 (White=5, Green=6, Red=7) -> rolls over 5 -> 6 -> 7 -> 5
                else // number is 5, 6, or 7
                {
                    nextTileNumber = (indicatorTile.Number == 7) ? 5 : indicatorTile.Number + 1;
                }
            }
            else // 数牌 (Manzu, Pinzu, Souzu)
            {
                // 1-8 -> number + 1
                // 9 -> 1
                nextTileNumber = (indicatorTile.Number == 9) ? 1 : indicatorTile.Number + 1;
            }

            return new Tile(nextTileSuit, nextTileNumber, 0);
        }

        /// <summary>
        /// 新しいドラ（カンが行われた場合など）を公開します。
        /// </summary>
        public void OpenNewDora()
        {
            int numberOfOpenedDora = Dora.Count; // すでに公開されているドラの数
            if (numberOfOpenedDora < 5) // 最大5つまで
            {
                // 次のドラ表示牌からドラを計算して追加
                Dora.Add(CalculateNextTile(DoraIndicator[numberOfOpenedDora]));

                // 対応する裏ドラも計算して追加
                // Check HiddenDoraIndicator bounds as well
                if (numberOfOpenedDora < HiddenDoraIndicator.Count)
                {
                    HiddenDora.Add(CalculateNextTile(HiddenDoraIndicator[numberOfOpenedDora]));
                }
            }
        }
    }
}