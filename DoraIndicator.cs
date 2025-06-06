
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection.PortableExecutable;
using System.Windows.Forms.VisualStyles;


namespace MahjongApp
{
    public class DoraIndicator : Control
    {
        const int MaxDoraCount = 5;
        Point startPos;
        Size panelSize;
        int radius;
        private Control parentControl;
        private List<PictureBox> doraPictureBoxes;
        // Wall wall;
        private Func<Wall> GetWallCallback;  // 牌が選択されたことを通知するコールバック

        public DoraIndicator(Control parentControl, Func<Wall> getWallCallback)
        {
            this.parentControl = parentControl;
            this.GetWallCallback = getWallCallback;
            this.DoubleBuffered = true;
            this.Paint += PaintBackGraound;
            radius = 5;
            panelSize = new Size(Config.Instance.DiscardTileWidth * 5 + 10 + 2, (int)(Config.Instance.DiscardTileHeight + 10) + 2);
            this.Location = new Point(20, 20);
            startPos = new Point(21, 21);
            this.Size = panelSize;
            this.parentControl.Controls.Add(this);

            doraPictureBoxes = new List<PictureBox>(5);

            InitializeDoraIndicator();
        }

        private void InitializeDoraIndicator()
        {
            doraPictureBoxes.Clear();
            for (int i = 0; i < MaxDoraCount; i++)
            {
                var pb = new PictureBox
                {
                    Size = new Size(Config.Instance.DiscardTileWidth, Config.Instance.DiscardTileHeight),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Visible = false,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = null,
                    BackColor = Color.Transparent,
                };
                doraPictureBoxes.Add(pb);
                parentControl.Controls.Add(pb);
                pb.BringToFront();
            }

            Debug.WriteLine($"[DoraIndicator] Initialized {MaxDoraCount} PictureBoxes.");
        }

        public void RefreshDisplay()
        {
            Wall? wall = GetWallCallback?.Invoke();
            if (wall == null)
            {
                Debug.WriteLine("[DoraIndicator ERROR] Wall not found for UI update.");
                // 全ての牌を非表示にするなどの処理
                foreach (var pb in doraPictureBoxes) pb.Visible = false;
                return;
            }

            int openedDora = wall.Dora.Count;
            List<Tile> doraIndicator = wall.DoraIndicator;

            for (int i = 0; i < MaxDoraCount; i++)
            {
                var pb = doraPictureBoxes[i];
                if (i < openedDora)
                {
                    Tile tile = doraIndicator[i];
                    pb.Image = TileImageCache.GetImage(tile);
                    pb.Tag = tile;
                    pb.Location = new Point(
                        startPos.X + i * (Config.Instance.DiscardTileWidth) + 5,
                        startPos.Y + 5
                    // startPos.Y + Config.Instance.DiscardTileHeight / 4
                    );
                    pb.Visible = true;
                }
                else
                {
                    pb.Visible = false;
                    pb.Tag = null;
                    pb.Image = null;
                }
            }
            this.Invalidate();
        }

        private void PaintBackGraound(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Black, 1);

            DrawRoundedRectangle(g, new SolidBrush(Color.Black), new Point(0, 0), panelSize, radius);
            DrawRoundedRectangle(g, new SolidBrush(Color.DarkGreen), new Point(1, 1), new Size(panelSize.Width - 2, panelSize.Height - 2), radius);
        }

        private void DrawRoundedRectangle(Graphics g, Brush brush, Point startPos, Size size, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int x = startPos.X;
            int y = startPos.Y;
            int width = size.Width;
            int height = size.Height;

            // 左上の角
            path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            // 上辺
            path.AddLine(x + radius, y, x + width - radius, y);
            // 右上の角
            path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
            // 右辺
            path.AddLine(x + width, y + radius, x + width, y + height - radius);
            // 右下の角
            path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
            // 下辺
            path.AddLine(x + width - radius, y + height, x + radius, y + height);
            // 左下の角
            path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
            // 左辺
            path.AddLine(x, y + height - radius, x, y + radius);

            // パスを閉じる
            path.CloseFigure();

            // 描画
            g.FillPath(brush, path);
        }

    }
}