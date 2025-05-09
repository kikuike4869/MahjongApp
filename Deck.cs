namespace MahjongApp
{
    class Deck
    {
        private List<Tile> Tiles;
        public List<Tile> Dora { get; private set; } = new();
        public List<Tile> HiddenDora { get; private set; } = new();
        public List<Tile> DoraIndicator { get; private set; } = new();
        public List<Tile> HiddenDoraIndicator { get; private set; } = new();
        public List<Tile> WanPie { get; private set; } = new();
        public int Count => Tiles.Count;
        private Random random;

        public Deck()
        {
            Tiles = new List<Tile>();
            Dora = new List<Tile>();
            HiddenDora = new List<Tile>();
            DoraIndicator = new List<Tile>();
            HiddenDoraIndicator = new List<Tile>();
            WanPie = new List<Tile>();

            random = new Random();

            GenerateTiles();
            Shuffle();

            SetDora();
            SetHiddenDora();
            SetWanPie();
        }

        private void GenerateTiles()
        {
            Suit[] suits = { Suit.Manzu, Suit.Pinzu, Suit.Souzu };
            foreach (var suit in suits)
            {
                for (int number = 1; number <= 9; number++)
                {
                    int copies_num = (number == 5) ? 3 : 4;
                    for (int i = 0; i < copies_num; i++)
                    {
                        Tiles.Add(new Tile(suit, number));
                    }
                }
            }

            for (int i = 1; i <= 7; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Tiles.Add(new Tile(Suit.Honor, i));
                }
            }

            // Add red fives (Man5r, Pin5r, Sou5r)
            Tiles.Add(new Tile(Suit.Manzu, 5, true));
            Tiles.Add(new Tile(Suit.Pinzu, 5, true));
            Tiles.Add(new Tile(Suit.Souzu, 5, true));
        }

        public void Shuffle()
        {
            for (int i = Tiles.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (Tiles[i], Tiles[j]) = (Tiles[j], Tiles[i]);
            }
        }
        public Tile Draw()
        {
            if (Tiles.Count == 0)
            {
                throw new InvalidOperationException("No Tiles left in the deck.");
            }

            Tile drawnTile = Tiles[0];
            Tiles.RemoveAt(0);
            return drawnTile;
        }

        public Tile DrawFromWanPie()
        {
            if (WanPie.Count == 0)
            {
                throw new InvalidOperationException("No Tiles left in the wanpie.");
            }

            Tile drawnTile = WanPie[0];
            WanPie.RemoveAt(0);
            return drawnTile;
        }


        private void SetDora()
        {
            for (int i = 0; i < 5; i++)
                DoraIndicator.Add(Draw());

            Dora.Add(NextTile(DoraIndicator[0]));
        }

        private void SetHiddenDora()
        {
            for (int i = 0; i < 5; i++)
                HiddenDoraIndicator.Add(Draw());

            HiddenDora.Add(NextTile(HiddenDoraIndicator[0]));
        }

        private Tile NextTile(Tile tile)
        {
            int nextTileNumber;
            Suit nextTileSuit = tile.Suit;

            if (tile.Suit == Suit.Honor)
            {
                if (tile.Number <= 4) nextTileNumber = tile.Number == 4 ? 1 : tile.Number + 1;
                else nextTileNumber = tile.Number == 7 ? 5 : tile.Number + 1;
            }
            else
            {
                nextTileNumber = tile.Number == 9 ? 1 : tile.Number + 1;
            }

            return new Tile(nextTileSuit, nextTileNumber);
        }

        private void SetWanPie()
        {
            for (int i = 0; i < 5; i++)
                WanPie.Add(Draw());
        }

        public void OpenDora()
        {
            int numberOfOpenedDora = Dora.Count();

            Dora.Add(NextTile(DoraIndicator[numberOfOpenedDora]));
            HiddenDora.Add(NextTile(HiddenDoraIndicator[numberOfOpenedDora]));
        }
    }
}