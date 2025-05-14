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
            int IndicatorSize = 40; // このサイズは固定で良いでしょう
            seatWindIndicators = new List<SeatWindIndicator>();
            var angles = new List<float> { 0, 270, 180, 90 }; // 東、南、西、北 の順に対応 (0:自分(下), 1:右, 2:対面(上), 3:左)

            // 描画順を考慮して逆順 (北から) で追加または、BringToFront/SendToBackで調整
            for (int i = 0; i < 4; i++)
            {
                var seatWindIndicator = new SeatWindIndicator()
                {
                    LabelText = Winds[i],    // 初期テキスト (Updateで上書きされる)
                    RotationAngle = angles[i], // 初期角度 (Updateで変わるものではない)
                    TextColor = Color.LightGray,
                    LabelFont = new Font("HGP行書体", 20, FontStyle.Bold),
                    Size = new Size(IndicatorSize, IndicatorSize),
                    BackgroundColor = Color.Black
                };
                seatWindIndicators.Add(seatWindIndicator);
                this.Controls.Add(seatWindIndicator); // 自分自身 (SeatWindIndicators) の Controls に追加
                seatWindIndicator.BringToFront();
            }

            this.SizeChanged += (sender, e) => LayoutIndicators();
        }

        public void LayoutIndicators()
        {
            if (seatWindIndicators == null || seatWindIndicators.Count != 4) return;

            int indicatorSize = seatWindIndicators[0].Width; // IndicatorSizeは同じ
            int parentWidth = this.Width;   // SeatWindIndicators自身の幅
            int parentHeight = this.Height; // SeatWindIndicators自身の高さ

            int centerPanelEdgeLength = this.Width; // もし正方形なら this.Height も同じ
            if (parentWidth == 0 || parentHeight == 0) return; // サイズ未確定の場合は何もしない

            // offset は、親パネルの端からどれだけ内側に配置するか
            int offset = indicatorSize / 3; // 元のコードに近いオフセット

            seatWindIndicators[0].Location = new Point(offset, centerPanelEdgeLength - indicatorSize - offset); // 左下 (東)
            seatWindIndicators[1].Location = new Point(centerPanelEdgeLength - indicatorSize - offset, centerPanelEdgeLength - indicatorSize - offset); // 右下 (南)
            seatWindIndicators[2].Location = new Point(centerPanelEdgeLength - indicatorSize - offset, offset); // 右上 (西)
            seatWindIndicators[3].Location = new Point(offset, offset); // 左上 (北)


            foreach (var swi in seatWindIndicators)
            {
                swi.Invalidate(); // 再描画を促す
            }
        }

        public void UpdateSeatWindIndicators(List<Wind> seatWinds, int dealerSeat)
        {
            if (seatWindIndicators == null || seatWindIndicators.Count != 4 || seatWinds == null || seatWinds.Count != 4)
            {
                // Debug.WriteLine("[SeatWindIndicators] Update failed: Invalid state or arguments.");
                return;
            }

            for (int i = 0; i < seatWindIndicators.Count; i++)
            {
                // i は表示位置に対応 (0:下, 1:右, 2:上, 3:左 と仮定)
                // seatWinds[i] はその位置のプレイヤーの自風
                Wind playerWind = seatWinds[i]; // seatWinds はプレイヤーの物理的な席順 (0:自分, 1:右, 2:対面, 3:左) の自風リストであるべき
                seatWindIndicators[i].LabelText = this.Winds[(int)playerWind];

                if (i == dealerSeat) // i が親の SeatIndex と一致する場合
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