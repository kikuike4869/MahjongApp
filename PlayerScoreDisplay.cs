// PlayerScoreDisplay.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq; // GetPlayersCallback().ToList() などで使う可能性

namespace MahjongApp
{
    public class PlayerScoreDisplays : Control
    {
        private List<PlayerScoreDisplay> playerScoreDisplays;
        private const int NumberOfPlayers = 4; // 通常は4人

        // レイアウト用の定数 (必要に応じてConfigから取得または調整)
        private int ScoreDisplayWidth = Config.Instance.DiscardTileWidth * 4; // 点数表示の幅
        private int ScoreDisplayHeight = Config.Instance.DiscardTileWidth * 1;  // 点数表示の高さ
        private int MarginFromEdge = 10;      // 画面端からのマージン
        // private int MarginFromDiscardArea = 5; // 捨て牌エリアとのマージン

        public PlayerScoreDisplays()
        {
            playerScoreDisplays = new List<PlayerScoreDisplay>();
            // 回転角度: 0:自家(下), 1:下家(右), 2:対面(上), 3:上家(左)
            // UI上の席順に合わせる
            var angles = new List<float> { 0, 270, 0, 90 }; // 上下は回転なし、左右は回転あり

            for (int i = 0; i < NumberOfPlayers; i++)
            {
                var playerScoreDisplay = new PlayerScoreDisplay()
                {
                    LabelText = "25000", // 初期値
                    RotationAngle = angles[i],
                    TextColor = Color.White,
                    LabelFont = new Font("Arial", 12, FontStyle.Bold), // 見やすいフォントに変更
                    Size = new Size((angles[i] == 0 || angles[i] == 180) ? ScoreDisplayWidth : ScoreDisplayHeight,
                                     (angles[i] == 0 || angles[i] == 180) ? ScoreDisplayHeight : ScoreDisplayWidth), // 回転を考慮したサイズ
                    BackgroundColor = Color.DimGray
                };
                playerScoreDisplays.Add(playerScoreDisplay);
                this.Controls.Add(playerScoreDisplay);
            }
            // 親コントロールのSizeChangedでレイアウト更新、または明示的に呼び出し
            this.SizeChanged += (sender, e) => LayoutDisplays();
        }

        public void LayoutDisplays()
        {
            if (this.Parent == null || playerScoreDisplays == null || playerScoreDisplays.Count != NumberOfPlayers) return;

            Size parentSize = this.Parent.ClientSize; // 親コントロール(例: MainForm)のサイズ

            // 画面下 (自家 - Player 0)
            playerScoreDisplays[0].Location = new Point(0, ScoreDisplayHeight - Config.Instance.DiscardTileWidth);
            playerScoreDisplays[0].Size = new Size(ScoreDisplayWidth, ScoreDisplayHeight);


            // 画面右 (下家 - Player 1)
            // 捨て牌エリアの左隣、または画面右端
            // DiscardWallStartPositions[1] と DiscardWallRotations[1] を参照したいが、ここでは直接アクセスできない。
            // MainFormから渡されたレイアウト情報や、Configの値を元に計算する。
            // 仮に画面右端に配置
            playerScoreDisplays[1].Size = new Size(ScoreDisplayHeight, ScoreDisplayWidth); // 回転後のサイズ
            playerScoreDisplays[1].Location = new Point(ScoreDisplayWidth - Config.Instance.DiscardTileWidth, 0);

            // 画面上 (対面 - Player 2)
            playerScoreDisplays[2].Location = new Point(0, 0);
            playerScoreDisplays[2].Size = new Size(ScoreDisplayWidth, ScoreDisplayHeight);


            // 画面左 (上家 - Player 3)
            // 仮に画面左端に配置
            playerScoreDisplays[3].Size = new Size(ScoreDisplayHeight, ScoreDisplayWidth); // 回転後のサイズ
            playerScoreDisplays[3].Location = new Point(0, 0);

            foreach (var psd in playerScoreDisplays)
            {
                psd.Invalidate();
            }
        }

        public void UpdateScores(List<Player>? players)
        {
            if (players == null || players.Count != NumberOfPlayers || playerScoreDisplays == null || playerScoreDisplays.Count != NumberOfPlayers)
            {
                //System.Diagnostics.Debug.WriteLine("[PlayerScoreDisplays] Update failed: Invalid state or arguments.");
                return;
            }

            for (int i = 0; i < NumberOfPlayers; i++)
            {
                // players リストはUI基準の並び (0:自家, 1:下家, 2:対面, 3:上家)
                // playerScoreDisplays も同じ順序で作成されている
                if (i < players.Count && playerScoreDisplays[i] != null)
                {
                    playerScoreDisplays[i].LabelText = players[i].Points.ToString();
                    playerScoreDisplays[i].Invalidate(); // 再描画を促す
                }
            }
        }
    }

    public class PlayerScoreDisplay : Control
    {
        public float RotationAngle { get; set; } = 0;
        public string LabelText { get; set; } = "25000";
        public Font LabelFont { get; set; } = new Font("Arial", 12, FontStyle.Bold);
        public Color TextColor { get; set; } = Color.White;
        public Color BackgroundColor { get; set; } = Color.FromArgb(150, 0, 0, 0); // 半透明の黒

        public PlayerScoreDisplay()
        {
            this.DoubleBuffered = true; // 描画のちらつきを軽減
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent; // 背景を透明に
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit; // テキストもアンチエイリアス

            // 背景の描画 (角丸四角形など、より凝ったデザインも可能)
            using (Brush backgroundBrush = new SolidBrush(this.BackgroundColor)) // クラスのプロパティを使用
            {
                // g.FillRectangle(backgroundBrush, 0, 0, this.Width, this.Height);
                // 角丸にする場合 (System.Drawing.Drawing2D.GraphicsPath を使用)
                using (var path = GetRoundedRectPath(new Rectangle(0, 0, this.Width, this.Height), 10)) // 10は角の半径
                {
                    g.FillPath(backgroundBrush, path);
                }
            }

            // テキストの描画位置と回転の中心を設定
            PointF center = new PointF(this.Width / 2f, this.Height / 2f);
            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(this.RotationAngle); // クラスのプロパティを使用

            // テキスト描画
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (Brush textBrush = new SolidBrush(this.TextColor)) // クラスのプロパティを使用
            {
                // 回転の中心に戻してから描画するために、(0,0) を中心とした座標系で描画する
                g.DrawString(this.LabelText, this.LabelFont, textBrush, 0, 0, sf); // クラスのプロパティを使用
            }

            g.ResetTransform(); // グラフィック状態を元に戻す
        }

        // 角丸の矩形パスを生成するヘルパーメソッド
        private static System.Drawing.Drawing2D.GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(bounds.X + bounds.Width - (radius * 2), bounds.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(bounds.X + bounds.Width - (radius * 2), bounds.Y + bounds.Height - (radius * 2), radius * 2, radius * 2, 0, 90);
            path.AddArc(bounds.X, bounds.Y + bounds.Height - (radius * 2), radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}