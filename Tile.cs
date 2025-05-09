namespace MahjongApp
{
    public class Tile
    {
        public Suit Suit { get; private set; }
        public int Number { get; private set; }         // 1-9 for 数牌, 1-7 for 字牌 (東=1, 南=2, ..., 中=7)
        public bool IsRed { get; private set; }

        public Tile(Suit suit, int number, bool isRed = false)
        {
            Suit = suit;
            Number = number;
            IsRed = isRed;
        }

        public override string ToString()
        {
            return $"{Suit}_{Number}{(IsRed ? "_red" : "")}";
        }

        public Image GetImage()
        {
            string filePath = $"Resources/Tiles/{ToString()}.png";
            if (System.IO.File.Exists(filePath))
            {
                return Image.FromFile(filePath);
            }
            else
            {
                throw new FileNotFoundException($"Image file not found: {filePath}");
            }
        }
    }
}
