using System; // Added for Action
using System.Collections.Generic; // Added for List
using System.Diagnostics; // Added for Debug.WriteLine
using System.Threading.Tasks; // Added for potential async operations

namespace MahjongApp
{
    class TurnManager
    {
        private int CurrentTurnSeat;               // 現在の手番プレイヤー（0〜3）
        private int DealerSeat;                    // 親の席番号（0〜3）

        private List<Player> Players;
        private Wall Wall;
        private CallManager CallMgr; // Renamed from SallMgr for clarity
        private ScoreManager ScoreMgr;

        private int TurnCount;                     // 累積手番数（流局判定用）
        public Tile? LastDiscardedTile { get; private set; } // Track the last discarded tile for calls

        // Callback for UI updates
        private Action? RefreshHandDisplayCallback = null;
        // Callback to signal completion of human discard (used by GameManager)
        // private Action OnHumanDiscardCompleted = null; // This notification now happens via GameManager method

        public TurnManager(List<Player> players, Wall wall, CallManager callMgr, ScoreManager scoreMgr, int dealerSeat)
        {
            this.Players = players;
            this.Wall = wall;
            this.CallMgr = callMgr; // Use renamed variable
            this.ScoreMgr = scoreMgr;
            this.DealerSeat = dealerSeat;
            this.CurrentTurnSeat = dealerSeat; // Start with the dealer
        }

        /// <summary>
        /// 新しいラウンドを開始し、配牌を行います。
        /// </summary>
        public void StartNewRound()
        {
            Debug.WriteLine("[Turn] Starting New Round");
            TurnCount = 0;
            CurrentTurnSeat = DealerSeat; // 親からスタート (DealerSeatはUI基準のインデックス)
            LastDiscardedTile = null;

            Wall.InitializeWall();

            int initialHandSize = Config.Instance.NumberOfFirstHands; // Configから取得
            foreach (Player player in Players) // PlayersリストはUI基準の並び
            {
                player.Hand.Clear();
                player.Discards.Clear();
                player.Melds.Clear();
                player.IsRiichi = false;
                player.IsTsumo = false;

                for (int i = 0; i < initialHandSize; i++)
                {
                    if (Wall.Count > 0)
                        player.Draw(Wall.Draw());
                    else
                        Debug.WriteLine("[ERROR] Wall empty during initial draw!");
                }
                player.IsTsumo = false;
                player.SortHand();
            }
            Debug.WriteLine("[Turn] Initial hands dealt.");
        }

        // StartTurn, DiscardByAI, DiscardTile, NextTurn, EndRound, IsHumanTurn は
        // CurrentTurnSeat がUI基準のインデックスであることを意識すれば、大きな変更は不要。

        public int GetCurrentTurnSeat() // UI基準のSeatIndexを返す
        {
            return CurrentTurnSeat;
        }

        public int GetDealerSeat() // UI基準のSeatIndexを返す
        {
            return DealerSeat;
        }

        // GameManagerからの親変更に対応するため
        public void SetDealerSeat(int dealerSeatIndex)
        {
            this.DealerSeat = dealerSeatIndex;
            this.CurrentTurnSeat = dealerSeatIndex;
        }

        /// <summary>
        /// 現在のプレイヤーのターンを開始し、牌を1枚ツモります。
        /// </summary>
        public void StartTurn()
        {
            Debug.WriteLine($"[Turn] StartTurn() called for Player {CurrentTurnSeat}. Wall: {Wall.Count}");

            if (Wall.Count == 0)
            {
                Debug.WriteLine("[Turn] Wall is empty, cannot draw. Triggering EndRound.");
                EndRound(); // Or trigger draw condition check
                return;
            }

            Player currentPlayer = Players[CurrentTurnSeat];
            Tile drawnTile = Wall.Draw();
            currentPlayer.Draw(drawnTile); // Player's Draw method sets IsTsumo = true
            Debug.WriteLine($"[Turn] Player {CurrentTurnSeat} drew: {drawnTile.Name()}");
            // RefreshHandDisplayCallback?.Invoke(); // GameManager triggers refresh after StartTurn

            // --- Check for self-actions after draw ---
            // Check Tsumo win condition here
            // Check self-Kan (Ankan, Kakan) options here
            // bool canTsumo = CheckWinCondition(currentPlayer, drawnTile, true);
            // if (canTsumo) { /* Trigger win handling in GameManager */ return; }
            // List<KanOption> kanOptions = CheckSelfKanOptions(currentPlayer, drawnTile);
            // if (kanOptions.Count > 0) { /* Present options or auto-declare based on AI/settings */ }

        }

        /// <summary>
        /// AIプレイヤーに打牌を選択させ、実行します。
        /// </summary>
        public void DiscardByAI()
        {
            Player currentPlayer = Players[CurrentTurnSeat];
            if (currentPlayer.IsHuman || currentPlayer.Hand.Count == 0)
            {
                Debug.WriteLine($"[Turn ERROR] DiscardByAI called for Human or empty hand.");
                return; // Should not happen
            }

            // 1. AI chooses a tile
            Tile? tileToDiscard = currentPlayer.ChooseDiscardTile();

            if (tileToDiscard != null)
            {
                Debug.WriteLine($"[Turn] AI Player {CurrentTurnSeat} chose to discard: {tileToDiscard.Name()}");
                // 2. Execute the discard
                DiscardTile(currentPlayer, tileToDiscard);
            }
            else
            {
                Debug.WriteLine($"[Turn ERROR] AI Player {CurrentTurnSeat} failed to choose a discard tile.");
                // Handle error - maybe discard last drawn tile?
                if (currentPlayer.Hand.Count > 0)
                {
                    DiscardTile(currentPlayer, currentPlayer.Hand.LastOrDefault()); // Fallback
                }
            }
        }

        /// <summary>
        /// 指定されたプレイヤーが指定された牌を捨てます。
        /// </summary>
        /// <param name="player">打牌するプレイヤー。</param>
        /// <param name="tileToDiscard">捨てる牌。</param>
        public void DiscardTile(Player player, Tile? tileToDiscard)
        {
            if (player == null || tileToDiscard == null) return;

            player.DiscardTile(tileToDiscard); // Use Player's method to update hand/discards
            LastDiscardedTile = tileToDiscard; // Track the discarded tile
            Debug.WriteLine($"[Turn] Player {player.SeatIndex} discarded: {LastDiscardedTile.Name()}");

            // Check for Ron after discard? (Logic for other players)
            // CheckCalls(LastDiscardedTile); // GameManager might handle this phase transition
        }


        /// <summary>
        /// 次のプレイヤーにターンを移します。
        /// </summary>
        public void NextTurn()
        {
            Debug.WriteLine($"[Turn] NextTurn() called. CurrentTurnSeat BEFORE: {CurrentTurnSeat}");
            if (Players.Count > 0) // Prevent division by zero/modulo error
            {
                Players[CurrentTurnSeat].IsTsumo = false; // Reset Tsumo state for the player whose turn just ended
                CurrentTurnSeat = (CurrentTurnSeat + 1) % Players.Count;
                TurnCount++; // Increment turn count
                LastDiscardedTile = null; // Reset last discard for the new turn
                Debug.WriteLine($"[Turn] NextTurn() finished. CurrentTurnSeat AFTER: {CurrentTurnSeat}, TurnCount: {TurnCount}");
            }
        }

        public void EndRound()
        {
            Debug.WriteLine("[Turn] End Round triggered.");
            // Determine if win, draw (exhaustive, abortive), etc.
            // Calculate scores via ScoreManager
            // Update Honba/Riichi sticks
            // Determine next dealer
            // Reset state for next round or trigger game over
        }

        /// <summary>
        /// 現在が人間プレイヤーのターンかどうかを返します。
        /// </summary>
        public bool IsHumanTurn()
        {
            if (Players.Count > CurrentTurnSeat && CurrentTurnSeat >= 0)
            {
                // Debug.WriteLine($"[Turn] Checking IsHumanTurn: Seat={CurrentTurnSeat}, IsHuman={Players[CurrentTurnSeat].IsHuman}");
                return Players[CurrentTurnSeat].IsHuman;
            }
            return false; // Invalid state
        }


        /// <summary>
        /// UI更新用のコールバックを設定します。
        /// </summary>
        public void SetUpdateUICallBack(Action refreshHandDisplay)
        {
            RefreshHandDisplayCallback = refreshHandDisplay;
        }

        // Removed SetHumanPlayerDiscardCallback and NotifyHumanDiscard
        // GameManager now handles waiting for human input via its own mechanism


        // --- Add methods for Game Logic ---
        // public bool CheckWinCondition(Player player, Tile checkTile, bool isTsumo) { /* ... */ return false; }
        // public List<KanOption> CheckSelfKanOptions(Player player, Tile drawnTile) { /* ... */ return new List<KanOption>(); }
        // public async Task<bool> CheckCalls(Tile discardedTile) { /* Check Pon/Kan/Chi/Ron from others */ return false; }

    }
}