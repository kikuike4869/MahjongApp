// GameStatusDisplayManager.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace MahjongApp
{
    public class GameStatusDisplayManager
    {
        private Panel? centerPanel; // 中央情報表示用のパネルは残しても良いでしょう
        private SeatWindIndicators? seatWindIndicators; // MainFormに直接配置する
        private WindAndRoundIndicator? lblCurrentWindAndRound;
        private RemainingTileIndicator? lblRemainingTiles;
        private PlayerScoreDisplays? playerScoreDisplays; // MainFormに直接配置する

        private Control parentControl;

        private Func<List<Wind>> GetSeatWindsCallback;
        private Func<int> GetDealerSeatCallback;
        private Func<Wind> GetCurrentWindCallback;
        private Func<int> GetCurrentRoundCallback;
        private Func<int> GetRemainingTilesCallback;
        private Func<List<Player>?> GetPlayersCallback;

        public GameStatusDisplayManager(
            Control parent,
            Func<List<Wind>> seatWindsCb,
            Func<int> dealerSeatCb,
            Func<Wind> currentWindCb,
            Func<int> currentRoundCb,
            Func<int> remainingTilesCb,
            Func<List<Player>?> playersCb)
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
            // --- 中央パネルの初期化 (場風、局、残り牌数表示用) ---
            int panelWidth = Config.Instance.DiscardTileWidth * 6; // このパネルは引き続き中央UI要素に使用
            int panelHeight = Config.Instance.DiscardTileWidth * 6;
            centerPanel = new Panel
            {
                Size = new Size(panelWidth, panelHeight),
                Location = new Point(
                    (parentControl.ClientSize.Width - panelWidth) / 2,
                    (parentControl.ClientSize.Height - panelHeight - Config.Instance.TileHeight) / 2 // 手牌スペースを考慮
                ),
                // BackColor = Color.Transparent, // 必要に応じて
                BackColor = Color.Transparent
            };
            parentControl.Controls.Add(centerPanel);
            centerPanel.SendToBack(); // 他のUI要素より奥に配置 (PlayerScoreDisplaysやSeatWindIndicatorsが手前になるように)


            // --- PlayerScoreDisplays の初期化 (parentControl に直接追加) ---
            playerScoreDisplays = new PlayerScoreDisplays
            {
                Parent = this.parentControl, // 親を MainForm に設定
                // サイズや位置は PlayerScoreDisplays 側で親に合わせて調整される想定
                // 必要であれば、ここで明示的に Size と Location を設定
                Size = new Size(Config.Instance.DiscardTileWidth * 4, Config.Instance.DiscardTileWidth * 4), // 仮のサイズ
            };
            // 中央に配置する例 (centerPanel と同様のロジック)
            playerScoreDisplays.Location = new Point(
                (parentControl.ClientSize.Width - Config.Instance.DiscardTileWidth * 4) / 2,
                (parentControl.ClientSize.Height - Config.Instance.DiscardTileWidth * 4 - Config.Instance.TileHeight) / 2
            );

            // this.parentControl.Controls.Add(playerScoreDisplays);
            // playerScoreDisplays.BringToFront(); // 他のUI要素より手前に配置

            foreach (var psd in playerScoreDisplays.GetPlayerScoreDisplays())
            {
                this.centerPanel.Controls.Add(psd);
                psd.SendToBack(); // centerPanel内で最前面に配置
            }


            // --- SeatWindIndicators の初期化 (parentControl に直接追加) ---
            seatWindIndicators = new SeatWindIndicators
            {
                Parent = this.parentControl, // 親を MainForm に設定
                                             // サイズや位置は SeatWindIndicators 側で親に合わせて調整される想定
                                             // 必要であれば、ここで明示的に Size と Location を設定
                Size = new Size(panelWidth, panelHeight), // 仮のサイズ、centerPanelと同じにするなど
            };
            // 中央に配置する例 (centerPanel と同様のロジック)
            seatWindIndicators.Location = new Point(
                (parentControl.ClientSize.Width - seatWindIndicators.Width) / 2,
                (parentControl.ClientSize.Height - seatWindIndicators.Height - Config.Instance.TileHeight) / 2
            );
            // this.parentControl.Controls.Add(seatWindIndicators);
            // seatWindIndicators.BringToFront(); // PlayerScoreDisplaysよりもさらに手前に配置、またはその逆など調整

            foreach (var swi in seatWindIndicators.GetSeatWindControls())
            {
                this.centerPanel.Controls.Add(swi);
                swi.BringToFront(); // centerPanel内で最前面に配置
            }

            // --- centerPanel 内の要素 (場風、局、残り牌数) ---
            lblCurrentWindAndRound = new WindAndRoundIndicator();
            lblCurrentWindAndRound.Location = new Point(
                (centerPanel.Width - lblCurrentWindAndRound.Width) / 2,
                (centerPanel.Height / 2) - lblCurrentWindAndRound.Height
            );
            centerPanel.Controls.Add(lblCurrentWindAndRound);
            lblCurrentWindAndRound.BringToFront(); // centerPanel内で最前面

            lblRemainingTiles = new RemainingTileIndicator();
            lblRemainingTiles.Location = new Point(
                (centerPanel.Width - lblRemainingTiles.Width) / 2,
                (centerPanel.Height / 2)
            );
            centerPanel.Controls.Add(lblRemainingTiles);
            lblRemainingTiles.BringToFront(); // centerPanel内で最前面


            parentControl.Resize += (sender, e) =>
            {
                // centerPanelの位置更新
                centerPanel.Location = new Point(
                    (parentControl.ClientSize.Width - centerPanel.Width) / 2,
                    (parentControl.ClientSize.Height - centerPanel.Height - Config.Instance.TileHeight) / 2
                );

                // PlayerScoreDisplays の位置とレイアウト更新
                if (playerScoreDisplays != null)
                {
                    // playerScoreDisplays.Size = parentControl.ClientSize; // 親全体に広げる場合
                    // 必要に応じて位置も再計算
                    playerScoreDisplays.Location = new Point(
                        (parentControl.ClientSize.Width - playerScoreDisplays.Width) / 2,
                        (parentControl.ClientSize.Height - playerScoreDisplays.Height - Config.Instance.TileHeight) / 2
                    );
                    playerScoreDisplays.LayoutDisplays();
                }

                // SeatWindIndicators の位置とレイアウト更新
                if (seatWindIndicators != null)
                {
                    // seatWindIndicators.Size = parentControl.ClientSize; // 親全体に広げる場合
                    // 必要に応じて位置も再計算
                    seatWindIndicators.Location = new Point(
                       (parentControl.ClientSize.Width - seatWindIndicators.Width) / 2,
                       (parentControl.ClientSize.Height - seatWindIndicators.Height - Config.Instance.TileHeight) / 2
                   );
                    seatWindIndicators.LayoutIndicators();
                }

                // centerPanel 内のコントロールも必要に応じて再配置
                if (lblCurrentWindAndRound != null)
                {
                    lblCurrentWindAndRound.Location = new Point(
                        (centerPanel.Width - lblCurrentWindAndRound.Width) / 2,
                        (centerPanel.Height / 2) - lblCurrentWindAndRound.Height
                    );
                }
                if (lblRemainingTiles != null)
                {
                    lblRemainingTiles.Location = new Point(
                        (centerPanel.Width - lblRemainingTiles.Width) / 2,
                        (centerPanel.Height / 2)
                    );
                }
            };
            // 初期レイアウトの実行
            playerScoreDisplays.LayoutDisplays();
            seatWindIndicators.LayoutIndicators();
        }

        public void RefreshDisplay()
        {
            if (parentControl.InvokeRequired)
            {
                parentControl.Invoke(new Action(RefreshDisplay));
                return;
            }

            // seatWindIndicators と playerScoreDisplays は MainForm 直下になったため、
            // それぞれの Update メソッドを呼び出す
            playerScoreDisplays?.UpdateScores(GetPlayersCallback?.Invoke());
            seatWindIndicators?.UpdateSeatWindIndicators(GetSeatWindsCallback.Invoke(), GetDealerSeatCallback.Invoke());

            // centerPanel 内の要素の更新は変更なし
            lblCurrentWindAndRound?.UpdateIndicator(GetCurrentWindCallback.Invoke(), GetCurrentRoundCallback.Invoke());
            lblRemainingTiles?.UpdateRemainingTiles(GetRemainingTilesCallback?.Invoke() ?? -1);
        }

        public void ClearControls()
        {
            // Resize イベントハンドラの解除は MainForm 側で行うのがより安全です

            if (playerScoreDisplays != null)
            {
                parentControl.Controls.Remove(playerScoreDisplays);
                playerScoreDisplays.Dispose();
                playerScoreDisplays = null;
            }
            if (seatWindIndicators != null)
            {
                parentControl.Controls.Remove(seatWindIndicators);
                seatWindIndicators.Dispose();
                seatWindIndicators = null;
            }
            // if (doraIndicator != null)
            // {
            //     parentControl.Controls.Remove(doraIndicator);
            //     doraIndicator.Dispose();
            //     doraIndicator = null;
            // }
            if (centerPanel != null)
            {
                parentControl.Controls.Remove(centerPanel);
                centerPanel.Dispose();
                centerPanel = null;
            }
            // lblCurrentWindAndRound, lblRemainingTiles は centerPanel の Dispose で処理される
        }
    }
}