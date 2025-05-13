// DiscardWallDisplayManager.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace MahjongApp
{
    public class DiscardWallDisplayManager
    {
        private Dictionary<int, List<PictureBox>> DiscardWallPictureBoxes = new Dictionary<int, List<PictureBox>>();
        private const int MaxDiscardTilesPerPlayer = 24; // 6x4

        // UI Layout Constants
        private int DiscardTileWidth;
        private int DiscardTileHeight;
        private int DiscardColumns;

        // 各プレイヤーの捨て牌表示の基点と回転情報 (MainFormから渡される)
        private Dictionary<int, Point> DiscardWallStartPositions;
        private Dictionary<int, bool> DiscardWallRotations; // trueなら90度回転

        private Control ParentControl;
        private Func<List<Player>?> GetPlayersCallback; // 全プレイヤーを取得するコールバック

        public DiscardWallDisplayManager(
            Control parentControl,
            Func<List<Player>?> getPlayersCallback,
            // レイアウト定数と設定
            int discardTileWidth, int discardTileHeight, int discardColumns,
            Dictionary<int, Point> discardWallStartPositions,
            Dictionary<int, bool> discardWallRotations
            )
        {
            ParentControl = parentControl;
            GetPlayersCallback = getPlayersCallback;

            DiscardTileWidth = discardTileWidth;
            DiscardTileHeight = discardTileHeight;
            DiscardColumns = discardColumns;
            DiscardWallStartPositions = discardWallStartPositions;
            DiscardWallRotations = discardWallRotations;

            InitializeDiscardWallPictureBoxes();
        }

        private void InitializeDiscardWallPictureBoxes()
        {
            DiscardWallPictureBoxes.Clear();
            // GetPlayersCallbackを使ってプレイヤー数を取得するか、固定値で初期化
            // ここでは、DiscardWallStartPositions のキーに基づいて初期化
            foreach (int playerIndex in DiscardWallStartPositions.Keys)
            {
                var pbs = new List<PictureBox>();
                for (int i = 0; i < MaxDiscardTilesPerPlayer; i++)
                {
                    var pb = new PictureBox
                    {
                        // 回転を考慮する前の基本サイズ
                        Size = new Size(DiscardTileWidth, DiscardTileHeight),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Visible = false,
                        BorderStyle = BorderStyle.None,
                        Tag = null,
                        BackColor = Color.Transparent,
                    };
                    pbs.Add(pb);
                    ParentControl.Controls.Add(pb);
                }
                DiscardWallPictureBoxes.Add(playerIndex, pbs);
            }
            Debug.WriteLine($"[DiscardWallManager] Initialized PictureBoxes for {DiscardWallStartPositions.Count} players.");
        }

        public void RefreshDisplay()
        {
            List<Player>? players = GetPlayersCallback?.Invoke();
            if (players == null || !players.Any())
            {
                Debug.WriteLine("[DiscardWallManager ERROR] No players found for discard wall update.");
                foreach (var pair in DiscardWallPictureBoxes)
                {
                    foreach (var pb in pair.Value) pb.Visible = false;
                }
                return;
            }

            foreach (var player in players)
            {
                if (!DiscardWallPictureBoxes.ContainsKey(player.SeatIndex) ||
                    !DiscardWallStartPositions.ContainsKey(player.SeatIndex) ||
                    !DiscardWallRotations.ContainsKey(player.SeatIndex))
                {
                    continue;
                }

                List<PictureBox> playerDiscardPbs = DiscardWallPictureBoxes[player.SeatIndex];
                List<Tile> discards = player.Discards;

                Point startPos = DiscardWallStartPositions[player.SeatIndex];
                bool rotate = DiscardWallRotations[player.SeatIndex];

                for (int i = 0; i < MaxDiscardTilesPerPlayer; i++)
                {
                    var pb = playerDiscardPbs[i];
                    if (i < discards.Count)
                    {
                        Tile tile = discards[i];
                        Image tileImage = TileImageCache.GetImage(tile);
                        Image? originalImageForPb = pb.Image; // Dispose漏れ対策用
                        Image? imageToDisplay = tileImage;

                        if (rotate)
                        {
                            Image rotatedImage = (Image)tileImage.Clone();
                            if (player.SeatIndex == 1) rotatedImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            else if (player.SeatIndex == 3) rotatedImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            // rotatedImage は後でDisposeが必要かもしれないので注意 (pb.Image に代入されたものが対象)
                            imageToDisplay = rotatedImage;
                            pb.Size = new Size(DiscardTileHeight, DiscardTileWidth);
                        }
                        else if (player.SeatIndex == 2) // 対面
                        {
                            Image rotatedImage = (Image)tileImage.Clone();
                            rotatedImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            imageToDisplay = rotatedImage;
                            pb.Size = new Size(DiscardTileWidth, DiscardTileHeight);
                        }
                        else // 自分
                        {
                            pb.Size = new Size(DiscardTileWidth, DiscardTileHeight);
                        }

                        // 以前の画像がクローンならDispose (現在のTileImageCacheの管理外のため)
                        if (originalImageForPb != null && originalImageForPb != TileImageCache.GetImage((Tile)pb.Tag!) && originalImageForPb != imageToDisplay)
                        {
                            originalImageForPb.Dispose();
                        }
                        pb.Image = imageToDisplay;


                        pb.Tag = tile;
                        pb.Location = CalculateDiscardTileLocation(i, startPos, rotate, player.SeatIndex);
                        pb.Visible = true;
                        ParentControl.Controls.SetChildIndex(pb, 0); // 最背面に
                    }
                    else
                    {
                        if (pb.Image != null && pb.Tag != null) // 以前表示されていたのが回転画像だった場合など
                        {
                            // TileImageCacheにない画像(Cloneされたもの)はDispose
                            // if (pb.Image != TileImageCache.GetImage((Tile)pb.Tag)) pb.Image.Dispose();
                            // より安全には、pb.Imageがキャッシュされたインスタンスそのものでない場合のみDispose
                            // ただし、TileImageCache.GetImageが常に同じインスタンスを返す保証がないと難しい
                            // ここでは、回転・クローンされたものは都度Disposeするのが無難か検討。
                            // 今回は、pb.Imageに代入されたものがTileImageCacheの元画像かクローンかを区別し、
                            // クローンだった場合に限り、次の描画で上書きされる前にDisposeすることを考える。
                            // 上記の imageToDisplay 代入前に originalImageForPb をチェックするロジックで対応試行。
                        }
                        pb.Visible = false;
                        pb.Tag = null;
                        // pb.Image = null; // 画像をnullに設定
                    }
                }
            }
        }

        private Point CalculateDiscardTileLocation(int index, Point startPosition, bool isRotated, int seatIndex)
        {
            int col = index % DiscardColumns;
            int row = index / DiscardColumns;
            int x, y;

            if (isRotated)
            {
                if (seatIndex == 1) // 右
                {
                    x = startPosition.X + row * DiscardTileHeight;
                    y = startPosition.Y - col * DiscardTileWidth;
                }
                else // 左 (seatIndex == 3)
                {
                    x = startPosition.X - row * DiscardTileHeight;
                    y = startPosition.Y + col * DiscardTileWidth;
                }
            }
            else
            {
                if (seatIndex == 2) // 対面
                {
                    x = startPosition.X + (DiscardColumns - 1 - col) * DiscardTileWidth;
                    y = startPosition.Y - row * DiscardTileHeight;
                }
                else // 自分 (SeatIndex 0)
                {
                    x = startPosition.X + col * DiscardTileWidth;
                    y = startPosition.Y + row * DiscardTileHeight;
                }
            }
            return new Point(x, y);
        }

        public void ClearPictureBoxes()
        {
            foreach (var pair in DiscardWallPictureBoxes)
            {
                foreach (var pb in pair.Value)
                {
                    ParentControl.Controls.Remove(pb);
                    if (pb.Image != null && pb.Tag != null) // 回転などでクローンされたイメージの可能性
                    {
                        // TileImageCache外の画像ならDispose
                        // この判別が難しい場合は、明示的にDisposeしないか、
                        // MainForm_FormClosingでまとめて行う方が安全
                    }
                    pb.Dispose();
                }
            }
            DiscardWallPictureBoxes.Clear();
        }
    }
}