// SeatWindIndicator.cs
namespace MahjongApp
{
    public class SeatWindIndicators : Control
    {
        List<SeatWindIndicator> seatWindIndicators;
        List<string> Winds = new List<string> { "東", "南", "西", "北" };

        int DiscardTileWidth = Config.Instance.DiscardTileWidth;
        int ScreenWidth = Config.Instance.ScreenSize.Width;
        int ScreenHeight = Config.Instance.ScreenSize.Height;

        public SeatWindIndicators()
        {
            int IndicatorSize = 40;
            seatWindIndicators = new List<SeatWindIndicator>();
            var angles = new List<float> { 0, 270, 180, 90 };
            // 4方向のプレイヤーための風インジケーター
            for (int i = 0; i < 4; i++)
            {
                var seatWindIndicator = new SeatWindIndicator()
                {
                    LabelText = Winds[i],
                    RotationAngle = angles[i],
                    TextColor = Color.LightGray,
                    LabelFont = new Font("HGP行書体", 20, FontStyle.Bold),
                    Size = new Size(IndicatorSize, IndicatorSize),
                    BackgroundColor = Color.Black
                };
                seatWindIndicators.Add(seatWindIndicator);
                Controls.Add(seatWindIndicator);
                // seatWindIndicators[i].Invalidate();
                seatWindIndicator.BringToFront();
            }

            int centerX = ScreenWidth / 2;
            int centerY = ScreenHeight / 2;

            // 配置：東（Bottom）、南（Right）、西（Top）、北（Left）
            // seatWindIndicators[0].Location = new Point(0, DiscardTileWidth * 6 - IndicatorSize);
            // seatWindIndicators[1].Location = new Point(DiscardTileWidth * 6 - IndicatorSize, DiscardTileWidth * 6);
            // seatWindIndicators[2].Location = new Point(DiscardTileWidth * 6 - IndicatorSize, IndicatorSize);
            // seatWindIndicators[3].Location = new Point(IndicatorSize, 0);
            seatWindIndicators[0].Location = new Point(0, DiscardTileWidth * 6 - IndicatorSize);
            seatWindIndicators[1].Location = new Point(DiscardTileWidth * 6 - IndicatorSize, DiscardTileWidth * 6 - IndicatorSize);
            seatWindIndicators[2].Location = new Point(DiscardTileWidth * 6 - IndicatorSize, 0);
            seatWindIndicators[3].Location = new Point(0, 0);
        }

        public void UpdateSeatWindIndicators(List<Wind> winds, int dealerSeat)
        {
            for (int i = 0; i < seatWindIndicators.Count; i++)
            {
                seatWindIndicators[i].LabelText = this.Winds[(int)winds[i]];
                if ((int)winds[i] == dealerSeat)
                {
                    seatWindIndicators[i].TextColor = Color.Black;
                    seatWindIndicators[i].BackgroundColor = Color.Red;
                }
                else
                {
                    seatWindIndicators[i].TextColor = Color.LightGray;
                    seatWindIndicators[i].BackgroundColor = Color.Black;
                }
                seatWindIndicators[i].Invalidate();
            }
        }
        public List<SeatWindIndicator> GetSeatWindControls() { return seatWindIndicators; }

    }

    public class SeatWindIndicator : Control
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