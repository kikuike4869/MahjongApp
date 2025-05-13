// GameStatusDisplayManager.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq; // プレイヤー情報の処理で使用する可能性

namespace MahjongApp
{
    public class GameStatusDisplayManager
    {
        private Panel statusPanel; // 全ての情報を表示するコンテナパネル
        private Label lblRoundInfo;      // 例: 東一局
        private Label lblHonbaRiichi;  // 例: 2本場 / 供託1本
        private Label lblCurrentTurn;    // 例: 手番: 山田 (東家)
        private Label lblRemainingTiles; // 例: 残り70枚

        // 各プレイヤーの情報を表示するラベルのリスト (またはDictionary)
        private Dictionary<int, Label> playerInfoLabels; // Key: SeatIndex

        private Control parentControl; // このマネージャが管理するUIを追加する先のコントロール

        // 情報取得のためのコールバック (MainForm経由でGameManagerなどから情報取得)
        private Func<string> GetRoundInfoTextCallback;
        private Func<string> GetHonbaRiichiTextCallback;
        private Func<string> GetCurrentTurnTextCallback;
        private Func<string> GetRemainingTilesTextCallback;
        private Func<List<Player>> GetPlayersCallback; // 全プレイヤーのリストを取得

        public GameStatusDisplayManager(
            Control parent,
            Func<string> roundInfoTextCb,
            Func<string> honbaRiichiTextCb,
            Func<string> currentTurnTextCb,
            Func<string> remainingTilesTextCb,
            Func<List<Player>> playersCb)
        {
            parentControl = parent;
            GetRoundInfoTextCallback = roundInfoTextCb;
            GetHonbaRiichiTextCallback = honbaRiichiTextCb;
            GetCurrentTurnTextCallback = currentTurnTextCb;
            GetRemainingTilesTextCallback = remainingTilesTextCb;
            GetPlayersCallback = playersCb;

            playerInfoLabels = new Dictionary<int, Label>();

            InitializeControls();
        }

        private void InitializeControls()
        {
            statusPanel = new Panel
            {
                // サイズや位置はMainFormから設定するか、ここで固定値を仮置き
                Size = new Size(300, 200), // 仮サイズ
                Location = new Point((parentControl.ClientSize.Width - 300) / 2, (parentControl.ClientSize.Height - 200) / 2), // 仮中央配置
                BackColor = Color.FromArgb(128, Color.LightGreen), // 半透明にして領域確認 (デバッグ用)
                // BorderStyle = BorderStyle.FixedSingle // デバッグ用
            };
            parentControl.Controls.Add(statusPanel);
            statusPanel.BringToFront(); // 河などの上に表示されるように

            // --- 各情報ラベルの初期化 ---
            lblRoundInfo = new Label { AutoSize = true, Location = new Point(10, 10), Font = new Font("Yu Gothic UI", 12F, FontStyle.Bold) };
            statusPanel.Controls.Add(lblRoundInfo);

            lblHonbaRiichi = new Label { AutoSize = true, Location = new Point(10, 35), Font = new Font("Yu Gothic UI", 10F) };
            statusPanel.Controls.Add(lblHonbaRiichi);

            lblCurrentTurn = new Label { AutoSize = true, Location = new Point(10, 60), Font = new Font("Yu Gothic UI", 10F) };
            statusPanel.Controls.Add(lblCurrentTurn);

            lblRemainingTiles = new Label { AutoSize = true, Location = new Point(statusPanel.Width - 100, 10), Font = new Font("Yu Gothic UI", 10F) }; // 右上に配置する例
            statusPanel.Controls.Add(lblRemainingTiles);

            // プレイヤー情報ラベルの初期化 (プレイヤー数に応じて動的に)
            List<Player> players = GetPlayersCallback?.Invoke() ?? new List<Player>();
            int playerLabelStartY = 90;
            int playerLabelSpacing = 25;
            foreach (Player player in players)
            {
                Label lblPlayer = new Label
                {
                    AutoSize = true,
                    Location = new Point(10, playerLabelStartY + (player.SeatIndex * playerLabelSpacing)), // 仮の配置
                    Font = new Font("Yu Gothic UI", 9F)
                };
                playerInfoLabels[player.SeatIndex] = lblPlayer;
                statusPanel.Controls.Add(lblPlayer);
            }
        }

        public void RefreshDisplay()
        {
            if (statusPanel.InvokeRequired)
            {
                statusPanel.Invoke(new Action(RefreshDisplay));
                return;
            }

            lblRoundInfo.Text = GetRoundInfoTextCallback?.Invoke() ?? "N/A";
            lblHonbaRiichi.Text = GetHonbaRiichiTextCallback?.Invoke() ?? "N/A";
            lblCurrentTurn.Text = GetCurrentTurnTextCallback?.Invoke() ?? "N/A";
            lblRemainingTiles.Text = GetRemainingTilesTextCallback?.Invoke() ?? "N/A";

            List<Player> players = GetPlayersCallback?.Invoke() ?? new List<Player>();
            foreach (Player player in players)
            {
                if (playerInfoLabels.TryGetValue(player.SeatIndex, out Label? label))
                {
                    // 例: "プレイヤー1 (東家): 25000点 [親]"
                    // 親かどうか、風などをプレイヤー情報から取得して表示する
                    string dealerMark = player.IsDealer ? "[親]" : "";
                    // TODO: プレイヤーの風を取得するロジック (親基準などで変わる)
                    label.Text = $"{player.Name}: {player.Points}点 {dealerMark}";
                }
            }
            // 必要に応じてラベルの位置を再調整 (AutoSize = true の場合など)
        }

        /// <summary>
        /// statusPanelの位置とサイズを調整します。MainFormのリサイズ時などに呼び出します。
        /// </summary>
        public void UpdateLayout(Size parentClientSize)
        {
            // statusPanelを中央に配置する例
            statusPanel.Location = new Point((parentClientSize.Width - statusPanel.Width) / 2,
                                            (parentClientSize.Height - statusPanel.Height) / 2);
            // 必要であれば、statusPanel内のラベルの再配置もここで行う
        }


        public void ClearControls()
        {
            if (statusPanel != null)
            {
                parentControl.Controls.Remove(statusPanel);
                statusPanel.Dispose();
                statusPanel = null;
            }
            playerInfoLabels.Clear(); // Label自体はPanelの子としてDisposeされる
        }
    }
}