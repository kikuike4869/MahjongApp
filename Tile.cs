namespace MahjongApp
{
    public class Tile
    {
        public Suit Suit { get; private set; }
        public int Index { get; private set; }
        public int Number { get; private set; }         // 1-9 for 数牌, 1-7 for 字牌 (東=1, 南=2, ..., 中=7)
        public bool IsRed { get; private set; }
        public bool IsSelected { get; set; }

        public Tile(Suit suit, int number, int index, bool isRed = false, bool isSelected = false)
        {
            Suit = suit;
            Number = number;
            Index = index;
            IsRed = isRed;
            IsSelected = isSelected;
        }

        public override string ToString()
        {
            return $"{Suit}_{Number}{(IsRed ? "_red" : "")}";
        }

        public string Name()
        {
            return $"{Suit}_{Number}_{Index}";
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
