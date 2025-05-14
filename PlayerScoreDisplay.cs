// SeatWindIndicator.cs

namespace MahjongApp
{
    public class PlayerScoreDisplays : Control
    {
        List<PlayerScoreDisplay> playerScoreDisplays;

        int DiscardTileWidth = Config.Instance.DiscardTileWidth;
        int DiscardTileHeight = Config.Instance.DiscardTileHeight;
        int ScreenWidth = Config.Instance.ScreenSize.Width;
        int ScreenHeight = Config.Instance.ScreenSize.Height;

        public PlayerScoreDisplays()
        {
            int IndicatorWidth = DiscardTileWidth * 5;
            int IndicatorHeight = DiscardTileWidth * 1;
            playerScoreDisplays = new List<PlayerScoreDisplay>();
            var angles = new List<float> { 0, 270, 180, 90 };

            for (int i = 0; i < 4; i++)
            {
                var playerScoreDisplay = new PlayerScoreDisplay()
                {
                    LabelText = "25000",
                    RotationAngle = angles[i],
                    TextColor = Color.LightGray,
                    LabelFont = new Font("HGP行書体", 20, FontStyle.Bold),
                    Size = new Size(IndicatorWidth, IndicatorHeight),
                    BackgroundColor = Color.Black
                };
                playerScoreDisplays.Add(playerScoreDisplay);
                Controls.Add(playerScoreDisplay);
                // playerScoreDisplays[i].Invalidate();
                playerScoreDisplay.BringToFront();
            }

            int centerX = ScreenWidth / 2;
            int centerY = ScreenHeight / 2;
            // int offset = IndicatorSize * 1 / 4;
            // int offset = IndicatorSize * 1 / 3;

            // 配置：東（Bottom）、南（Right）、西（Top）、北（Left）
            // playerScoreDisplays[0].Location = new Point(offset, DiscardTileWidth * 6 - IndicatorSize - offset);
            // playerScoreDisplays[1].Location = new Point(DiscardTileWidth * 6 - IndicatorSize - offset, DiscardTileWidth * 6 - IndicatorSize - offset);
            // playerScoreDisplays[2].Location = new Point(DiscardTileWidth * 6 - IndicatorSize - offset, offset);
            // playerScoreDisplays[3].Location = new Point(offset, offset);
        }

        // public void UpdateSeatWindIndicators(List<Wind> winds, int dealerSeat)
        // {
        //     for (int i = 0; i < playerScoreDisplays.Count; i++)
        //     {
        //         playerScoreDisplays[i].LabelText = this.Winds[(int)winds[i]];
        //         if ((int)winds[i] == dealerSeat)
        //         {
        //             playerScoreDisplays[i].TextColor = Color.Black;
        //             playerScoreDisplays[i].BackgroundColor = Color.Red;
        //         }
        //         else
        //         {
        //             playerScoreDisplays[i].TextColor = Color.LightGray;
        //             playerScoreDisplays[i].BackgroundColor = Color.Black;
        //         }
        //         playerScoreDisplays[i].Invalidate();
        //     }
        // }
        public List<PlayerScoreDisplay> GetSeatWindControls() { return playerScoreDisplays; }

    }

    public class PlayerScoreDisplay : Control
    {
        public float RotationAngle { get; set; } = 0;
        public string LabelText { get; set; } = "東";
        public Font LabelFont { get; set; } = new Font("HGP行書体", 16, FontStyle.Bold);
        public Color TextColor { get; set; } = Color.LightGray;
        public Color BackgroundColor { get; set; } = Color.Black;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (Brush backgroundBrush = new SolidBrush(BackgroundColor))
            {
                g.FillRectangle(backgroundBrush, new Rectangle(0, 0, this.Width, this.Height));
            }

            g.TranslateTransform(this.Width / 2, this.Height / 2);
            g.RotateTransform(RotationAngle);
            SizeF textSize = g.MeasureString(LabelText, LabelFont);
            using (Brush textBrush = new SolidBrush(TextColor))
            {
                g.DrawString(LabelText, LabelFont, textBrush, -textSize.Width / 2, -textSize.Height / 2);
            }

            g.ResetTransform();
        }
    }
}