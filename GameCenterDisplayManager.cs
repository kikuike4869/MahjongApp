// GameCenterDisplayManager.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace MahjongApp
{
    public class GameCenterDisplayManager
    {
        private Panel centerPanel; // 中央の情報用パネル

        private SeatWindIndicators lblSeatWinds;
        private WindAndRoundIndicator lblCurrentWindAndRound;
        private RemainingTileIndicator lblRemainingTiles;
        private PlayerScoreDisplays lblPlayerScores; // ★PlayerScoreDisplays を追加

        private Control parentControl; // このマネージャがUIを追加する先のコントロール (MainForm)

        private Func<List<Wind>> GetSeatWindsCallback;
        private Func<int> GetDealerSeatCallback;
        private Func<Wind> GetCurrentWindCallback;
        private Func<int> GetCurrentRoundCallback;
        private Func<int> GetRemainingTilesCallback;
        private Func<List<Player>?> GetPlayersCallback; // ★null許容に変更

        public GameCenterDisplayManager(
            Control parent,
            Func<List<Wind>> seatWindsCb,
            Func<int> dealerSeatCb,
            Func<Wind> currentWindCb,
            Func<int> currentRoundCb,
            Func<int> remainingTilesCb,
            Func<List<Player>?> playersCb) // ★null許容に変更
        {
            parentControl = parent;

            GetSeatWindsCallback = seatWindsCb;
            GetDealerSeatCallback = dealerSeatCb;
            GetCurrentWindCallback = currentWindCb;
            GetCurrentRoundCallback = currentRoundCb;
            GetRemainingTilesCallback = remainingTilesCb;
            GetPlayersCallback = playersCb;

            InitializeControls();
        }

        private void InitializeControls()
        {
            // --- 中央パネルの初期化 ---
            int panelWidth = Config.Instance.DiscardTileWidth * 6;
            int panelHeight = Config.Instance.DiscardTileWidth * 6;
            centerPanel = new Panel
            {
                Size = new Size(panelWidth, panelHeight),
                Location = new Point(
                    (parentControl.ClientSize.Width - panelWidth) / 2,
                    (parentControl.ClientSize.Height - panelHeight - Config.Instance.TileHeight) / 2
                    ),
                // BackColor = Color.DarkSlateGray, // デバッグ用
            };
            parentControl.Controls.Add(centerPanel);
            // centerPanel.BringToFront(); // 他要素との兼ね合いで調整

            // --- 中央パネル内の要素 ---
            lblSeatWinds = new SeatWindIndicators();
            lblSeatWinds.Size = new Size(panelWidth, panelHeight);
            centerPanel.Controls.Add(lblSeatWinds);


            // --- PlayerScoreDisplays の初期化 ---
            lblPlayerScores = new PlayerScoreDisplays
            {
                Parent = this.parentControl, // 親を MainForm に設定
                // Dock = DockStyle.Fill, // これだと centerPanel などと重なる可能性
                Size = this.parentControl.ClientSize, // MainForm全体を対象にレイアウトさせる
                BackColor = Color.Transparent // PlayerScoreDisplays自体の背景は透明に
            };
            lblPlayerScores.Location = new Point(
                (parentControl.ClientSize.Width - lblPlayerScores.Width) / 2,
                (parentControl.ClientSize.Height - lblPlayerScores.Height - Config.Instance.TileHeight) / 2
            );
            parentControl.Controls.Add(lblPlayerScores);
            lblPlayerScores.BringToFront(); // 他のUI要素（特にcenterPanel）より奥に配置する

            // // 初期レイアウトの実行
            // lblPlayerScores.LayoutDisplays();

            lblCurrentWindAndRound = new WindAndRoundIndicator();
            lblCurrentWindAndRound.Location = new Point(
                (centerPanel.Width - lblCurrentWindAndRound.Width) / 2,
                (centerPanel.Height / 2) - lblCurrentWindAndRound.Height
            );
            centerPanel.Controls.Add(lblCurrentWindAndRound);
            lblCurrentWindAndRound.BringToFront();

            lblRemainingTiles = new RemainingTileIndicator();
            lblRemainingTiles.Location = new Point(
                (centerPanel.Width - lblRemainingTiles.Width) / 2,
                (centerPanel.Height / 2)
            );
            centerPanel.Controls.Add(lblRemainingTiles);
            lblRemainingTiles.BringToFront();



            // MainForm のリサイズイベントを拾ってレイアウトを再実行できるようにする
            // (より堅牢なのは、MainForm側からこのManagerのUpdateLayoutを呼ぶこと)
            parentControl.Resize += (sender, e) =>
            {
                if (lblPlayerScores != null)
                {
                    lblPlayerScores.Size = parentControl.ClientSize; // サイズ追従
                    lblPlayerScores.LayoutDisplays();
                }
                // centerPanelの位置も再計算が必要ならここで行うか、専用メソッドで
                centerPanel.Location = new Point(
                    (parentControl.ClientSize.Width - centerPanel.Width) / 2,
                    (parentControl.ClientSize.Height - centerPanel.Height - Config.Instance.TileHeight) / 2
                );
            };
        }

        public void RefreshDisplay()
        {
            if (parentControl.InvokeRequired) // 親コントロールのInvokeRequiredをチェック
            {
                parentControl.Invoke(new Action(RefreshDisplay));
                return;
            }

            // 中央パネル内の要素の更新
            lblSeatWinds.UpdateSeatWindIndicators(GetSeatWindsCallback.Invoke(), GetDealerSeatCallback.Invoke());
            lblCurrentWindAndRound.UpdateIndicator(GetCurrentWindCallback.Invoke(), GetCurrentRoundCallback.Invoke());
            lblRemainingTiles.UpdateRemainingTiles(GetRemainingTilesCallback?.Invoke() ?? -1);

            // 点数表示の更新
            lblPlayerScores?.UpdateScores(GetPlayersCallback?.Invoke());
        }

        public void ClearControls()
        {
            // parentControl.Resize イベントハンドラの解除
            parentControl.Resize -= (sender, e) =>
            {
                if (lblPlayerScores != null)
                {
                    lblPlayerScores.Size = parentControl.ClientSize;
                    lblPlayerScores.LayoutDisplays();
                }
                centerPanel.Location = new Point( /* ... */ );
            };

            if (lblPlayerScores != null)
            {
                parentControl.Controls.Remove(lblPlayerScores);
                lblPlayerScores.Dispose();
                lblPlayerScores = null;
            }
            if (centerPanel != null)
            {
                parentControl.Controls.Remove(centerPanel);
                centerPanel.Dispose(); // centerPanel内部のコントロールも一緒にDisposeされる
                centerPanel = null;
            }
            // lblSeatWinds など、centerPanelの子だったものはcenterPanel.Dispose()で一緒に処理される
        }
    }
}