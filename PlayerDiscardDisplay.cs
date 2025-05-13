namespace MahjongApp
{
    public class PlayerDiscardDisplay
    {
        private const int DiscardTileWidth = 30; // 牌の幅
        private const int DiscardTileHeight = 40; // 牌の高さ
        private const int DiscardRows = 4; // 河の行数
        private const int DiscardColumns = 6; // 河の列数

        private List<PictureBox> discardPictureBoxes = new List<PictureBox>();
        private Point startPosition; // 河の開始位置
        private bool isRotated; // プレイヤーの河を回転させるかどうか

        public PlayerDiscardDisplay(Point startPosition, bool isRotated)
        {
            this.startPosition = startPosition;
            this.isRotated = isRotated;
            InitializeDiscardPictureBoxes();
        }

        private void InitializeDiscardPictureBoxes()
        {
            for (int i = 0; i < DiscardRows * DiscardColumns; i++)
            {
                var pb = new PictureBox
                {
                    Size = new Size(DiscardTileWidth, DiscardTileHeight),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Visible = false,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = null
                };
                discardPictureBoxes.Add(pb);
            }
        }

        public void RefreshDiscardDisplay(List<Tile> discards)
        {
            for (int i = 0; i < discardPictureBoxes.Count; i++)
            {
                var pb = discardPictureBoxes[i];
                if (i < discards.Count)
                {
                    Tile tile = discards[i];
                    pb.Image = TileImageCache.GetImage(tile);
                    if (isRotated)
                    {
                        pb.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    }
                    pb.Tag = tile;
                    pb.Location = CalculateDiscardTileLocation(i);
                    pb.Visible = true;
                }
                else
                {
                    pb.Visible = false;
                    pb.Tag = null;
                    pb.Image = null;
                }
            }
        }

        private Point CalculateDiscardTileLocation(int index)
        {
            int row = index / DiscardColumns;
            int col = index % DiscardColumns;
            int x = startPosition.X + col * (isRotated ? DiscardTileWidth : DiscardTileHeight);
            int y = startPosition.Y + row * (isRotated ? DiscardTileWidth : DiscardTileHeight);
            return new Point(x, y);
        }

        public List<PictureBox> GetPictureBoxes()
        {
            return discardPictureBoxes;
        }
    }
}