using System;
using System.Collections.Generic; // Added for List
using System.Diagnostics; // Added for Debug.WriteLine
using System.Drawing;
using System.Windows.Forms;
using System.Linq; // Added for LINQ operations if needed

namespace MahjongApp
{
    public partial class MainForm : Form
    {
        private MahjongGameManager gameManager;
        // PictureBoxを保持するリスト (ObservableTilePictureBoxはPictureBoxで代用)
        private List<PictureBox> HandTilePictureBoxes = new List<PictureBox>();
        private const int MaxHandTiles = 15; // Maximum possible tiles in hand (13 + 1 drawn)

        // UI Layout Constants (Consider making these configurable or dynamic)
        private const int TileWidth = 60; // Slightly smaller for default layout
        private const int TileHeight = 80;
        private int HandStartX = TileWidth;
        private int HandStartY = Config.Instance.ScreenSize.Height - TileHeight * 2; // Adjust based on form size
        private int SelectedOffsetY = TileHeight / 2; // How much selected tiles move up
        private int DrawnTileOffsetX = TileWidth; // Space between 13th and drawn tile
        private const int DiscardTileWidth = 30; // 牌の幅
        private const int DiscardTileHeight = 40; // 牌の高さ
        private const int DiscardRows = 4; // 河の行数
        private const int DiscardColumns = 6; // 河の列数

        public MainForm()
        {
            InitializeComponent();

            // Double Buffering to reduce flicker
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();


            InitializeHandTilePictureBoxes(); // Create PictureBoxes once

            gameManager = new MahjongGameManager();
            // RefreshHandDisplayをコールバックとして設定 (UI更新用)
            gameManager.SetUpdateUICallBack(RefreshHandDisplay);

            // ゲーム開始 (非同期的に実行される可能性がある)
            // Use Task.Run or similar if StartGame blocks for long or needs background thread
            gameManager.Test(); // Starts the game logic which should trigger the first RefreshHandDisplay

            // Register FormClosing event to dispose image cache
            this.FormClosing += MainForm_FormClosing;
        }

        /// <summary>
        /// 手牌表示用のPictureBoxを初期化し、フォームに追加します。
        /// </summary>
        private void InitializeHandTilePictureBoxes()
        {
            HandTilePictureBoxes.Clear(); // Just in case
            for (int i = 0; i < MaxHandTiles; i++)
            {
                var pb = new PictureBox
                {
                    Size = new Size(TileWidth, TileHeight),
                    SizeMode = PictureBoxSizeMode.StretchImage, // Adjust as needed
                    Visible = false, // Initially hidden
                    BorderStyle = BorderStyle.FixedSingle, // Optional: for visibility
                    Tag = null, // Will hold the Tile object
                    BackColor = Color.Transparent,
                    
                };
                pb.Click += TilePictureBox_Click; // Attach the single event handler
                HandTilePictureBoxes.Add(pb);
                this.Controls.Add(pb); // Add to form's controls
            }
            Debug.WriteLine($"[UI] Initialized {MaxHandTiles} PictureBoxes.");
        }

        /// <summary>
        /// プレイヤーの手牌に基づいてPictureBoxの表示を更新します。
        /// </summary>
        private void RefreshHandDisplay()
        {
            // Ensure UI updates run on the UI thread if called from background thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(RefreshHandDisplay));
                return;
            }

            HumanPlayer player = gameManager.GetHumanPlayer();
            if (player == null)
            {
                Debug.WriteLine("[UI ERROR] Human player not found for UI update.");
                return; // Or handle appropriately
            }

            List<Tile> hand = player.Hand;
            // player.SortHand(); // Ensure hand is sorted before display (unless Riichi)

            Debug.WriteLine($"[UI] Refreshing hand display for {hand.Count} tiles. IsTsumo: {player.IsTsumo}");

            // Update existing PictureBoxes
            for (int i = 0; i < MaxHandTiles; i++)
            {
                var pb = HandTilePictureBoxes[i];
                if (i < hand.Count)
                {
                    Tile tile = hand[i];
                    pb.Image = TileImageCache.GetImage(tile); // Use cached image
                    pb.Tag = tile; // Store Tile object in Tag
                    pb.Location = CalculateTileLocation(i, hand.Count, tile.IsSelected, player.IsTsumo);
                    pb.Visible = true;

                    // Optional: Visual cue for selected tile (besides position)
                    pb.BackColor = tile.IsSelected ? Color.LightBlue : SystemColors.Control;
                }
                else
                {
                    pb.Visible = false; // Hide unused PictureBoxes
                    pb.Tag = null;
                    pb.Image = null;
                }
            }
        }

        /// <summary>
        /// 各牌のPictureBoxの位置を計算します。
        /// </summary>
        /// <param name="index">牌のインデックス (0から)</param>
        /// <param name="handCount">現在の手牌の総数</param>
        /// <param name="isSelected">この牌が選択されているか</param>
        /// <param name="isTsumoState">プレイヤーがツモ直後か</param>
        /// <returns>PictureBoxの表示位置</returns>
        private Point CalculateTileLocation(int index, int handCount, bool isSelected, bool isTsumoState)
        {
            int selectedOffset = isSelected ? SelectedOffsetY : 0;
            // ツモ牌 (手牌が14枚目で、かつツモ状態) の場合に少し右にずらす
            int drawnTileOffset = (isTsumoState && index == handCount - 1 && handCount % 3 == 2) ? DrawnTileOffsetX : 0;
            // Note: Standard hand is 13 tiles, draw makes 14. Discard returns to 13.
            // The drawn tile offset logic might need refinement based on exact display rules.
            // Typically, the 14th tile is slightly separated.

            int x = HandStartX + index * TileWidth + drawnTileOffset;
            int y = HandStartY - selectedOffset;

            return new Point(x, y);
        }


        /// <summary>
        /// いずれかの牌PictureBoxがクリックされたときのイベントハンドラ。
        /// </summary>
        private void TilePictureBox_Click(object sender, EventArgs e)
        {
            if (sender is PictureBox clickedPb && clickedPb.Tag is Tile selectedTile)
            {
                HumanPlayer player = gameManager.GetHumanPlayer();
                if (player == null || !player.IsHuman) return; // Should not happen if UI is for human

                Debug.WriteLine($"[UI] Tile clicked: {selectedTile.Name()}, Current Phase: {gameManager.CurrentPhase}");

                bool IsDiscardActive = ((gameManager.CurrentPhase == GamePhase.DiscardPhase) && gameManager.IsHumanTurnFromTurnManager() && selectedTile.IsSelected);

                // 現在のゲームフェーズと牌の状態に応じて処理
                if (IsDiscardActive) // Check if it's discard phase
                {
                    Debug.WriteLine($"[UI] Discarding selected tile: {selectedTile.Name()}");
                    // Let GameManager/TurnManager handle the discard logic
                    player.DiscardTile(selectedTile); // Perform discard action on player object
                    RefreshHandDisplay(); // Update UI immediately after discard
                    gameManager.NotifyHumanDiscardOfTurnManager(); // Notify game logic discard is done
                }
                // If the clicked tile was not selected, select it (and deselect others)
                else
                {
                    Debug.WriteLine($"[UI] Selecting tile: {selectedTile.Name()}");
                    // Deselect all other tiles in hand
                    foreach (var tileInHand in player.Hand)
                    {
                        tileInHand.IsSelected = false;
                    }
                    // Select the clicked tile
                    selectedTile.IsSelected = true;
                    RefreshHandDisplay(); // Update UI to show selection change
                }

                // Allow selection changes even outside discard phase? Maybe not.
                // else if (selectedTile.IsSelected) // Allow de-selection anytime?
                // {
                //     selectedTile.IsSelected = false;
                //     RefreshHandDisplay();
                // }
            }
        }

        /// <summary>
        /// Form closing event to clean up resources.
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debug.WriteLine("[UI] Form closing, disposing image cache.");
            TileImageCache.DisposeCache(); // Dispose cached images
        }

        // Removed DisplayPlayerHand() as RefreshHandDisplay() now handles updates.
        // TileClickLogger class is removed and its logic integrated into TilePictureBox_Click.
    }

    // Assume Config class exists (or define basic version)
    // public static class Config { public static Config Instance = new Config(); public int NumberOfPlayers = 4; /* ... other settings ... */ }

    // Assume GamePhase enum exists (or define)
    // public enum GamePhase { InitRound, DiscardPhase, MakeDecision, RoundOver, GameOver }

    // Assume CallManager and ScoreManager exist (minimal definition for compilation)
    // public class CallManager { public CallManager(List<Player> p, int d) {} }
    // public class ScoreManager { public ScoreManager() {} }

    // Assume Suit enum exists (as used in Tile.cs)
    // public enum Suit { Manzu, Pinzu, Souzu, Honor }
}