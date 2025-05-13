// MainForm.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace MahjongApp
{
    public partial class MainForm : Form
    {
        private MahjongGameManager gameManager;
        private HandDisplayManager handDisplayManager;
        private DiscardWallDisplayManager discardWallDisplayManager;

        // UI Layout Constants (一部は各Managerに渡す)
        private const int TileWidth = 60;
        private const int TileHeight = 80;
        private int HandStartX = TileWidth;
        private int HandStartY; // 初期化は ClientSize が確定してからの方が良い場合も
        private int SelectedOffsetY = TileHeight / 2;
        private int DrawnTileOffsetX = TileWidth;

        private const int DiscardTileWidth = 36;
        private const int DiscardTileHeight = 48;
        private const int DiscardColumns = 6;

        private Dictionary<int, Point> DiscardWallStartPositions = new Dictionary<int, Point>();
        private Dictionary<int, bool> DiscardWallRotations = new Dictionary<int, bool>();

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load; // ClientSize確定後にレイアウト設定するため

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            // gameManagerの初期化
            gameManager = new MahjongGameManager();
            gameManager.SetUpdateUICallBack(RefreshHandDisplays, RefreshDiscardWallDisplays); // UI更新コールバック
            gameManager.SetEnableHandInteractionCallback(EnableHandInteraction); // UI操作可否コールバック

            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // HandStartY は ClientSize.Height を使って決定
            HandStartY = this.ClientSize.Height - TileHeight * 2;
            if (HandStartY < TileHeight) HandStartY = TileHeight + 10; // 画面が小さすぎる場合のフォールバック

            // DisplayManager の初期化 (親コントロールとして this (Form) を渡す)
            handDisplayManager = new HandDisplayManager(
                this, // 親コントロール
                () => gameManager.GetHumanPlayer(),
                HandleTileDiscard,  // 打牌処理のメソッドを渡す
                HandleTileSelect,   // 選択処理のメソッドを渡す
                TileWidth, TileHeight, HandStartX, HandStartY, SelectedOffsetY, DrawnTileOffsetX
            );

            SetupDiscardWallLayout(gameManager.GetPlayers()?.Count ?? 4); // プレイヤー数に基づいてレイアウト設定

            discardWallDisplayManager = new DiscardWallDisplayManager(
                this, // 親コントロール
                () => gameManager.GetPlayers(),
                DiscardTileWidth, DiscardTileHeight, DiscardColumns,
                DiscardWallStartPositions, DiscardWallRotations
            );

            gameManager.Test(); // ゲーム開始 (DisplayManager初期化後)
        }


        private void SetupDiscardWallLayout(int numberOfPlayers)
        {
            DiscardWallStartPositions.Clear();
            DiscardWallRotations.Clear();

            int centerX = this.ClientSize.Width / 2;
            int centerY = (this.ClientSize.Height - TileHeight) / 2;
            Size playerDiscardAreaSize = new Size(DiscardTileWidth * DiscardColumns, DiscardTileHeight * (24 / DiscardColumns)); // MaxDiscardTilesPerPlayer
            Size playerDiscardAreaSizeRotated = new Size(DiscardTileHeight * (24 / DiscardColumns), DiscardTileWidth * DiscardColumns);
            int marginFromCenter = DiscardTileWidth * 3;

            int OffsetY = 0;

            if (numberOfPlayers == 4)
            {
                // 自分 (0)
                DiscardWallStartPositions[0] = new Point(centerX - playerDiscardAreaSize.Width / 2, centerY + marginFromCenter + OffsetY);
                DiscardWallRotations[0] = false;
                // 右 (1)
                DiscardWallStartPositions[1] = new Point(centerX + marginFromCenter, centerY + playerDiscardAreaSize.Width / 2 - DiscardTileWidth + OffsetY);
                DiscardWallRotations[1] = true;
                // 対面 (2)
                DiscardWallStartPositions[2] = new Point(centerX - playerDiscardAreaSize.Width / 2, centerY - marginFromCenter - DiscardTileHeight + OffsetY);
                DiscardWallRotations[2] = false;
                // 左 (3)
                DiscardWallStartPositions[3] = new Point(centerX - marginFromCenter - DiscardTileHeight, centerY - playerDiscardAreaSize.Width / 2 + OffsetY);
                DiscardWallRotations[3] = true;
            }
            // 他のプレイヤー数のレイアウトも必要なら追加
        }

        private void RefreshDiscardWallDisplays()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(RefreshDiscardWallDisplays));
                return;
            }
            discardWallDisplayManager?.RefreshDisplay();
        }
        private void RefreshHandDisplays()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(RefreshHandDisplays));
                return;
            }
            handDisplayManager?.RefreshDisplay();
        }

        // HandDisplayManagerからのコールバック処理
        private void HandleTileDiscard(Tile tile)
        {
            Debug.WriteLine($"[MainForm] HandleTileDiscard: {tile.Name()}");
            if (gameManager.CurrentPhase == GamePhase.DiscardPhase && gameManager.IsHumanTurnFromTurnManager())
            {
                HumanPlayer? player = gameManager.GetHumanPlayer();
                if (player != null)
                {
                    player.DiscardTile(tile); // Modelの更新
                                              // RefreshAllDisplays(); // Model更新後、GameManagerからのコールバックでUIが更新されるはず
                                              // ただし、即時反映したい場合はここで呼ぶか、
                                              // GameManagerがDiscardTile後にUI更新をトリガーするようにする。
                                              // 現状はNotifyHumanDiscardOfTurnManager内でUI更新はされないので、ここで呼ぶ。
                    RefreshHandDisplays(); // 手牌表示を更新
                    RefreshDiscardWallDisplays(); // 捨て牌表示を更新
                    gameManager.NotifyHumanDiscardOfTurnManager(); // GameManagerに打牌完了を通知
                }
            }
        }

        private void HandleTileSelect(Tile tile)
        {
            Debug.WriteLine($"[MainForm] HandleTileSelect: {tile.Name()}");
            HumanPlayer? player = gameManager.GetHumanPlayer();
            if (player != null)
            {
                // 他の牌の選択を解除
                foreach (var t in player.Hand)
                {
                    t.IsSelected = false;
                }
                tile.IsSelected = true;
                handDisplayManager.RefreshDisplay(); // 手牌表示のみ更新
            }
        }

        /// <summary>
        /// GameManager から呼び出され、手牌操作のUIを有効/無効にします。
        /// </summary>
        private void EnableHandInteraction(bool enable)
        {
            // HandDisplayManager に PictureBox の Enabled を設定させるか、
            // MainForm が直接 HandTilePictureBoxes を知っていればここで設定できます。
            // 今回、HandDisplayManager が PictureBox を管理しているので、
            // HandDisplayManager にメソッドを追加するのが良いでしょう。
            // (例: handDisplayManager.SetInteraction(enable);)
            // ここでは簡略化のため、MainForm側では特に何もしないか、
            // HandDisplayManager内でクリックイベントハンドラ冒頭で判定する形でも対応可能。
            // もし厳密にPictureBox.Enabledを切り替えるならHandDisplayManagerにメソッド追加。
            Debug.WriteLine($"[MainForm] Hand interaction set to: {enable}");
            // 現状の HandDisplayManager の TilePictureBox_Click では、
            // 実際にアクションを行うかどうかを MainForm 側のコールバック(HandleTileDiscard/Select)
            // に委ね、そこから GameManager の状態を見て判断しているので、
            // PictureBox 自体の Enabled を切り替える必要性は薄いかもしれません。
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debug.WriteLine("[UI] Form closing, disposing resources.");
            handDisplayManager?.ClearPictureBoxes();
            discardWallDisplayManager?.ClearPictureBoxes();
            TileImageCache.DisposeCache();
        }
    }
}