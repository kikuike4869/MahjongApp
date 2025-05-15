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
        private GameCenterDisplayManager gameCenterDisplayManager;

        // UI Layout Constants (一部は各Managerに渡す)
        private int TileWidth = Config.Instance.TileWidth;
        private int TileHeight = Config.Instance.TileHeight;
        private int HandStartX = Config.Instance.TileWidth;
        private int HandStartY; // 初期化は ClientSize が確定してからの方が良い場合も
        private int SelectedOffsetY = Config.Instance.TileWidth / 2;
        private int DrawnTileOffsetX = Config.Instance.TileWidth;

        public int DiscardTileWidth = Config.Instance.DiscardTileWidth;
        public int DiscardTileHeight = Config.Instance.DiscardTileHeight;
        private const int DiscardColumns = 6;

        private Dictionary<int, Point> DiscardWallStartPositions = new Dictionary<int, Point>();
        private Dictionary<int, bool> DiscardWallRotations = new Dictionary<int, bool>();

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            // gameManagerの初期化
            gameManager = new MahjongGameManager();
            // ★変更点: InitializeUICallbacks を呼び出す
            gameManager.InitializeUICallbacks(
                RefreshHandDisplays,
                RefreshDiscardWallDisplays,
                RefreshGameCenterDisplays,
                EnableHandInteraction
            );

            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            HandStartY = this.ClientSize.Height - TileHeight * 2;
            if (HandStartY < TileHeight) HandStartY = TileHeight + 10;

            handDisplayManager = new HandDisplayManager(
                this,
                () => gameManager.GetHumanPlayer(), // GetHumanPlayerはMahjongGameManagerに実装想定
                HandleTileDiscard,
                HandleTileSelect,
                TileWidth, TileHeight, HandStartX, HandStartY, SelectedOffsetY, DrawnTileOffsetX
            );

            SetupDiscardWallLayout(gameManager.GetPlayers()?.Count ?? 4);

            discardWallDisplayManager = new DiscardWallDisplayManager(
                this,
                () => gameManager.GetPlayers(),
                DiscardTileWidth, DiscardTileHeight, DiscardColumns,
                DiscardWallStartPositions, DiscardWallRotations
            );

            gameCenterDisplayManager = new GameCenterDisplayManager(
                this,
                () => gameManager.GetSeatWinds(),
                () => gameManager.GetDealerSeat(),
                () => gameManager.GetCurrentWind(),
                () => gameManager.GetCurrentRound(),
                () => gameManager.GetRemainingTileCount(),
                () => gameManager.GetPlayers()
            );

            // ★変更点: ゲーム開始のトリガー
            gameManager.TriggerStartGameForTest(); // または StartNewGameAsync() を適切に呼び出す
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

        private void RefreshGameCenterDisplays()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(RefreshGameCenterDisplays));
                return;
            }
            gameCenterDisplayManager?.RefreshDisplay();
        }

        // HandDisplayManagerからのコールバック処理
        private void HandleTileDiscard(Tile tile)
        {
            Debug.WriteLine($"[MainForm] HandleTileDiscard: {tile.Name()}");
            HumanPlayer? player = gameManager.GetHumanPlayer();
            if (player != null && player.Hand.Contains(tile)) // 手牌にあるか確認
            {
                // 1. Playerモデルの更新 (手牌から削除し、捨て牌に追加)
                //    これはUI側の責務か、GameManagerに依頼するか設計による。
                //    ここではUI側（MainForm）でPlayerモデルを直接更新すると仮定。
                //    ただし、UIスレッドからのモデル操作は注意が必要。
                //    より安全なのは、GameManagerに「この牌を捨てたい」と通知し、
                //    GameManagerがモデルを更新し、UI更新コールバックをトリガーする形。
                //    今回は、既存の player.DiscardTile が Playerクラスに存在し、
                //    手牌と捨て牌を更新すると仮定する。
                player.DiscardTile(tile); // Playerモデルを更新

                // 2. UI表示の更新 (手牌と捨て牌)
                RefreshHandDisplays();
                RefreshDiscardWallDisplays();

                // 3. GameManagerに打牌完了を通知
                gameManager.NotifyHumanDiscard(tile);
            }
            else
            {
                Debug.WriteLine($"[MainForm WARNING] Attempted to discard tile not in hand or player is null: {tile.Name()}");
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
            gameCenterDisplayManager?.ClearControls();
            TileImageCache.DisposeCache();
        }
    }
}