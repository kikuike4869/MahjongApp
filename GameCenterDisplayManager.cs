// GameCenterDisplayManager.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq; // プレイヤー情報の処理で使用する可能性

namespace MahjongApp
{
    public class GameCenterDisplayManager
    {
        private Panel centerPanel; // 全ての情報を表示するコンテナパネル

        private SeatWindIndicators lblSeatWinds;
        private WindAndRoundIndicator lblCurrentWindAndRound;
        private RemainingTileIndicator lblRemainingTiles; // 例: 残り70枚


        private Control parentControl; // このマネージャが管理するUIを追加する先のコントロール

        // 情報取得のためのコールバック (MainForm経由でGameManagerなどから情報取得)
        private Func<List<Wind>> GetSeatWindsCallback;
        private Func<int> GetDealerSeatCallback;
        private Func<Wind> GetCurrentWindCallback;
        private Func<int> GetCurrentRoundCallback;
        private Func<int> GetRemainingTilesCallback;
        private Func<List<Player>> GetPlayersCallback; // 全プレイヤーのリストを取得

        public GameCenterDisplayManager(
            Control parent,
            Func<List<Wind>> seatWindsCb,
            Func<int> dealerSeatCb,
            Func<Wind> currentWindCb,
            Func<int> currentRoundCb,
            Func<int> remainingTilesCb,
            Func<List<Player>> playersCb)
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
            int panelWidth = Config.Instance.DiscardTileWidth * 6;
            int panelHeight = Config.Instance.DiscardTileWidth * 6;
            centerPanel = new Panel
            {
                // サイズや位置はMainFormから設定するか、ここで固定値を仮置き
                Size = new Size(panelWidth, panelHeight), // 仮サイズ
                Location = new Point((Config.Instance.ScreenSize.Width - panelWidth) / 2, (Config.Instance.ScreenSize.Height - panelHeight - Config.Instance.TileHeight) / 2),
                // BackColor = Color.FromArgb(128, Color.DarkSlateGray), // 半透明にして領域確認 (デバッグ用)
                // BackColor = Color.DarkSlateGray,
            };
            parentControl.Controls.Add(centerPanel);
            centerPanel.BringToFront(); // 河などの上に表示されるように

            // --- 各情報ラベルの初期化 ---

            lblSeatWinds = new SeatWindIndicators();
            List<SeatWindIndicator> lblSeatWindList = lblSeatWinds.GetSeatWindControls();
            foreach (var lblSeatWind in lblSeatWindList) { centerPanel.Controls.Add(lblSeatWind); }

            lblCurrentWindAndRound = new WindAndRoundIndicator();
            centerPanel.Controls.Add(lblCurrentWindAndRound);

            lblRemainingTiles = new RemainingTileIndicator();
            centerPanel.Controls.Add(lblRemainingTiles);
        }

        public void RefreshDisplay()
        {
            if (centerPanel.InvokeRequired)
            {
                centerPanel.Invoke(new Action(RefreshDisplay));
                return;
            }

            lblSeatWinds.UpdateSeatWindIndicators(GetSeatWindsCallback.Invoke(), GetDealerSeatCallback.Invoke());
            lblCurrentWindAndRound.UpdateIndicator(GetCurrentWindCallback.Invoke(), GetCurrentRoundCallback.Invoke());
            lblRemainingTiles.UpdateRemainingTiles(GetRemainingTilesCallback?.Invoke() ?? -1);
        }

        /// <summary>
        /// centerPanelの位置とサイズを調整します。MainFormのリサイズ時などに呼び出します。
        /// </summary>
        // public void UpdateLayout(Size parentClientSize)
        // {
        //     // centerPanelを中央に配置する例
        //     centerPanel.Location = new Point((parentClientSize.Width - centerPanel.Width) / 2,
        //                                     (parentClientSize.Height - centerPanel.Height) / 2);
        //     // 必要であれば、centerPanel内のラベルの再配置もここで行う
        // }


        public void ClearControls()
        {
            if (centerPanel != null)
            {
                parentControl.Controls.Remove(centerPanel);
                centerPanel.Dispose();
                centerPanel = null;
            }
        }
    }
}