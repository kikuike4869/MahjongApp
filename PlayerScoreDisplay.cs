// PlayerScoreDisplay.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D; // GraphicsPath のために追加
using System.Windows.Forms;
using System.Linq;

namespace MahjongApp
{
    // PlayerScoreDisplays クラス (変更なし)
    public class PlayerScoreDisplays : Label
    {
        private List<PlayerScoreDisplay> playerScoreDisplays;
        private const int NumberOfPlayers = 4;

        private int ScoreDisplayWidth = Config.Instance.DiscardTileWidth * 4;
        private int ScoreDisplayHeight = Config.Instance.DiscardTileWidth * 1;
        private int MarginFromEdge = 10;

        public PlayerScoreDisplays()
        {
            this.SetStyle(ControlStyles.UserPaint |              // コントロールが自身を描画
                          ControlStyles.AllPaintingInWmPaint |    // WM_ERASEBKGND を無視
                          ControlStyles.OptimizedDoubleBuffer |   // ダブルバッファリング
                          ControlStyles.ResizeRedraw |            // サイズ変更時に再描画
                          ControlStyles.SupportsTransparentBackColor, // 透明背景をサポート
                          true);
            this.BackColor = Color.Transparent;

            playerScoreDisplays = new List<PlayerScoreDisplay>();
            // UI上の席順に合わせる (0:自家(下), 1:下家(右), 2:対面(上), 3:上家(左))
            var angles = new List<float> { 0, 270, 180, 90 }; // 回転角度

            for (int i = 0; i < NumberOfPlayers; i++)
            {
                var playerScoreDisplay = new PlayerScoreDisplay()
                {
                    LabelText = "25000",
                    RotationAngle = angles[i],
                    TextColor = Color.White,
                    LabelFont = new Font("Arial", 12, FontStyle.Bold),
                    // 回転を考慮したサイズ (角度が0または180の場合は幅広、90または270の場合は高さ広)
                    Size = new Size(
                        (angles[i] == 0 || angles[i] == 180) ? ScoreDisplayWidth : ScoreDisplayHeight,
                        (angles[i] == 0 || angles[i] == 180) ? ScoreDisplayHeight : ScoreDisplayWidth
                    ),
                    // BackgroundColor プロパティは PlayerScoreDisplay 側で定義・使用
                    // BackColor プロパティは Control.BackColor を指すため、ここでは設定しない
                };
                playerScoreDisplays.Add(playerScoreDisplay);
                this.Controls.Add(playerScoreDisplay);
            }
            this.SizeChanged += (sender, e) => LayoutDisplays();
            // PlayerScoreDisplays 自体の背景を透明にするための設定
            // ただし、これが直接エラーの原因になることがある
            // this.BackColor = Color.Transparent; // エラーが出る場合はコメントアウトまたは削除
            // SetStyle(ControlStyles.SupportsTransparentBackColor, true); // これも効果がない場合がある
        }

        public List<PlayerScoreDisplay> GetPlayerScoreDisplays()
        {
            return playerScoreDisplays;
        }

        public void LayoutDisplays()
        {
            if (this.Parent == null || playerScoreDisplays == null || playerScoreDisplays.Count != NumberOfPlayers) return;

            Size parentSize = this.Parent.ClientSize; // 親コントロール(例: MainForm)のサイズ

            // 各 PlayerScoreDisplay のサイズと位置を調整
            // ここでの位置は、PlayerScoreDisplays コントロール内の相対位置になります。
            int offsetX = Config.Instance.DiscardTileWidth / 2;
            int offsetY = Config.Instance.DiscardTileWidth / 2;

            // プレイヤー0 (下 - 自家)
            playerScoreDisplays[0].Location = new Point(Config.Instance.DiscardTileWidth, Config.Instance.DiscardTileWidth * 4);

            // プレイヤー1 (右 - 下家)
            playerScoreDisplays[1].Location = new Point(Config.Instance.DiscardTileWidth * 4, Config.Instance.DiscardTileWidth);

            // プレイヤー2 (上 - 対面)
            playerScoreDisplays[2].Location = new Point(Config.Instance.DiscardTileWidth, Config.Instance.DiscardTileWidth);

            // プレイヤー3 (左 - 上家)
            playerScoreDisplays[3].Location = new Point(Config.Instance.DiscardTileWidth, Config.Instance.DiscardTileWidth);


            foreach (var psd in playerScoreDisplays)
            {
                psd.Invalidate();
            }
        }

        public void UpdateScores(List<Player>? players)
        {
            if (players == null || players.Count != NumberOfPlayers || playerScoreDisplays == null || playerScoreDisplays.Count != NumberOfPlayers)
            {
                return;
            }

            for (int i = 0; i < NumberOfPlayers; i++)
            {
                if (i < players.Count && playerScoreDisplays[i] != null)
                {
                    playerScoreDisplays[i].LabelText = players[i].Points.ToString();
                    playerScoreDisplays[i].Invalidate();
                }
            }
        }

        // PlayerScoreDisplays が破棄される際に、内部のコントロールも破棄する
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (playerScoreDisplays != null)
                {
                    foreach (var psd in playerScoreDisplays)
                    {
                        psd.Dispose();
                    }
                    playerScoreDisplays.Clear();
                }
            }
            base.Dispose(disposing);
        }
    }

    public class PlayerScoreDisplay : Label
    {
        public float RotationAngle { get; set; } = 0;
        public string LabelText { get; set; } = "25000";
        public Font LabelFont { get; set; } = new Font("HGP行書体", 12, FontStyle.Bold);
        public Color TextColor { get; set; } = Color.White;
        public Color DisplayBackgroundColor { get; set; } = Color.DarkSlateGray; // 背景色プロパティを変更
        // public Color DisplayBackgroundColor { get; set; } =  Color.Transparent;

        public PlayerScoreDisplay()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint |              // コントロールが自身を描画
                          ControlStyles.AllPaintingInWmPaint |    // WM_ERASEBKGND を無視
                          ControlStyles.OptimizedDoubleBuffer |   // ダブルバッファリング
                          ControlStyles.ResizeRedraw |            // サイズ変更時に再描画
                          ControlStyles.SupportsTransparentBackColor, // 透明背景をサポート
                          true);
            this.TextAlign = ContentAlignment.MiddleCenter; // テキストを中央揃え
            this.BorderStyle = BorderStyle.None; // 枠線を非表示
            this.BackColor = Color.Transparent; // コントロール自体の背景は透明に設定
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // base.OnPaint(e) は呼び出さないか、条件付きで呼び出す
            // UserPaint を true にしているので、描画はすべてここで行う

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            // ★背景の描画 (角の丸くない長方形)
            using (Brush backgroundBrush = new SolidBrush(this.DisplayBackgroundColor)) // プロパティで指定された色を使用
            {
                // FillRectangleで単純な長方形を描画
                g.FillRectangle(backgroundBrush, 0, 0, this.Width, this.Height);
            }

            // テキストの描画位置と回転の中心を設定
            PointF center = new PointF(this.Width / 2f, this.Height / 2f);
            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(this.RotationAngle);

            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (Brush textBrush = new SolidBrush(this.TextColor))
            {
                g.DrawString(this.LabelText, this.LabelFont, textBrush, 0, 0, sf);
            }

            g.ResetTransform();
        }
    }
}