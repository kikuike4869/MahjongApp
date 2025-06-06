// HandDisplayManager.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace MahjongApp
{
    public class HandDisplayManager
    {
        private List<PictureBox> HandTilePictureBoxes = new List<PictureBox>();
        private const int MaxHandTiles = 15;

        // UI Layout Constants (MainFormから渡すか、共通のConfigクラスを参照)
        private int TileWidth;
        private int TileHeight;
        private int HandStartX;
        private int HandStartY;
        private int SelectedOffsetY;
        private int DrawnTileOffsetX;

        private Control ParentControl; // PictureBoxを追加する親コントロール (Formなど)
        private Func<HumanPlayer?> GetHumanPlayerCallback; // 人間プレイヤーを取得するコールバック
        private Action<Tile> OnTileDiscardCallback; // 牌が捨てられたことを通知するコールバック
        private Action<Tile> OnTileSelectCallback;  // 牌が選択されたことを通知するコールバック

        public HandDisplayManager(
            Control parentControl,
            Func<HumanPlayer?> getHumanPlayerCallback,
            Action<Tile> onTileDiscardCallback,
            Action<Tile> onTileSelectCallback,
            // レイアウト定数をコンストラクタで受け取る
            int tileWidth, int tileHeight, int handStartX, int handStartY, int selectedOffsetY, int drawnTileOffsetX
            )
        {
            ParentControl = parentControl;
            GetHumanPlayerCallback = getHumanPlayerCallback;
            OnTileDiscardCallback = onTileDiscardCallback;
            OnTileSelectCallback = onTileSelectCallback;

            TileWidth = tileWidth;
            TileHeight = tileHeight;
            HandStartX = handStartX;
            HandStartY = handStartY;
            SelectedOffsetY = selectedOffsetY;
            DrawnTileOffsetX = drawnTileOffsetX;

            InitializeHandTilePictureBoxes();
        }

        private void InitializeHandTilePictureBoxes()
        {
            HandTilePictureBoxes.Clear();
            for (int i = 0; i < MaxHandTiles; i++)
            {
                var pb = new PictureBox
                {
                    Size = new Size(TileWidth, TileHeight),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Visible = false,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = null,
                    BackColor = Color.Transparent,
                };
                pb.Click += TilePictureBox_Click;
                HandTilePictureBoxes.Add(pb);
                ParentControl.Controls.Add(pb); // 親コントロールにPictureBoxを追加
            }
            Debug.WriteLine($"[HandDisplayManager] Initialized {MaxHandTiles} PictureBoxes.");
        }

        public void RefreshDisplay()
        {
            HumanPlayer? player = GetHumanPlayerCallback?.Invoke();
            if (player == null)
            {
                Debug.WriteLine("[HandDisplayManager ERROR] Human player not found for UI update.");
                // 全ての牌を非表示にするなどの処理
                foreach (var pb in HandTilePictureBoxes) pb.Visible = false;
                return;
            }

            List<Tile> hand = player.Hand;
            // player.SortHand(); // 描画前にソートが必要な場合はここで (Playerクラス側で管理されている想定)

            Debug.WriteLine($"[HandDisplayManager] Refreshing hand display for {hand.Count} tiles. IsTsumo: {player.IsTsumo}");

            for (int i = 0; i < MaxHandTiles; i++)
            {
                var pb = HandTilePictureBoxes[i];
                if (i < hand.Count)
                {
                    Tile tile = hand[i];
                    pb.Image = TileImageCache.GetImage(tile);
                    pb.Tag = tile;
                    pb.Location = CalculateTileLocation(i, hand.Count, tile.IsSelected, player.IsTsumo);
                    pb.Visible = true;
                }
                else
                {
                    pb.Visible = false;
                    pb.Tag = null;
                    pb.Image = null;
                }
            }
            ParentControl.Invalidate();
        }

        private Point CalculateTileLocation(int index, int handCount, bool isSelected, bool isTsumoState)
        {
            int selectedOffset = isSelected ? SelectedOffsetY : 0;
            int drawnTileOffset = (isTsumoState && index == handCount - 1 && handCount % 3 == 2) ? DrawnTileOffsetX : 0;
            int x = HandStartX + index * TileWidth + drawnTileOffset;
            int y = HandStartY - selectedOffset;
            return new Point(x, y);
        }

        private void TilePictureBox_Click(object? sender, EventArgs e)
        {
            if (sender is PictureBox clickedPb && clickedPb.Tag is Tile selectedTile)
            {
                HumanPlayer? player = GetHumanPlayerCallback?.Invoke();
                if (player == null) return;

                // GameManagerから現在のフェーズやターン情報を取得する必要がある
                // ここでは簡略化のため、コールバック経由で親 (MainForm) に通知し、
                // 親がGameManagerと連携して判断・処理する。
                if (selectedTile.IsSelected) // 既に選択されている牌をクリックしたら打牌とする
                {
                    selectedTile.IsSelected = false;
                    RefreshDisplay();
                    OnTileDiscardCallback?.Invoke(selectedTile);
                }
                else // 選択されていなければ選択する
                {
                    OnTileSelectCallback?.Invoke(selectedTile);
                }
            }
        }

        public void ClearPictureBoxes()
        {
            foreach (var pb in HandTilePictureBoxes)
            {
                ParentControl.Controls.Remove(pb);
                pb.Click -= TilePictureBox_Click;
                pb.Dispose();
            }
            HandTilePictureBoxes.Clear();
        }
    }
}