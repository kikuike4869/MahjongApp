// MahjongGameManager.cs (修正後)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq; // FirstOrDefaultのために追加
using System.Threading.Tasks;

namespace MahjongApp
{
    public class MahjongGameManager
    {
        List<Player> Players;
        Deck Deck;
        TurnManager TurnManager;
        CallManager CallManager;
        ScoreManager ScoreManager;
        public GamePhase CurrentPhase = GamePhase.InitRound;

        // int RoundNumber;
        // int HonbaCount;
        // int RiichiSticks;
        int DealerIndex;

        // Callback for UI updates
        Action? RefreshHandDisplayCallback;
        // Callback for enabling/disabling UI hand interaction
        Action<bool>? EnableHandInteractionCallback; // <<< 追加

        public MahjongGameManager()
        {
            Deck = new Deck();
            Players = new List<Player>();
            InitializeGame();
            CallManager = new CallManager(Players, DealerIndex);
            ScoreManager = new ScoreManager();
            TurnManager = new TurnManager(Players, Deck, CallManager, ScoreManager, DealerIndex);
            TurnManager.SetUpdateUICallBack(() => RefreshHandDisplayCallback?.Invoke());
        }

        void InitializeGame()
        {
            DealerIndex = 0;
            int numPlayers = 4;
            if (Players == null) Players = new List<Player>(numPlayers);
            Players.Clear();
            for (int i = 0; i < numPlayers; i++)
            {
                Player newPlayer;
                if (i == 0) { newPlayer = new HumanPlayer { SeatIndex = i, Name = $"Player {i} (Human)" }; }
                else { newPlayer = new AIPlayer { SeatIndex = i, Name = $"Player {i} (AI)" }; }
                newPlayer.IsDealer = (i == DealerIndex);
                Players.Add(newPlayer);
            }
            Debug.WriteLine($"[Game] Initialized {Players.Count} players. Dealer is Player {DealerIndex}.");
        }

        public async Task StartGame()
        {
            Debug.WriteLine("[Game] Starting game...");
            CurrentPhase = GamePhase.InitRound;
            TurnManager.StartNewRound();
            EnableHandInteractionCallback?.Invoke(false); // <<< 初期状態は操作不可
            RefreshHandDisplayCallback?.Invoke();

            while (Deck.Count > 0 && CurrentPhase != GamePhase.GameOver)
            {
                try
                {
                    Debug.WriteLine($"[Game] ----- Turn Start: Player {TurnManager.GetCurrentTurnSeat()}, Deck: {Deck.Count} -----");
                    CurrentPhase = GamePhase.DrawPhase;
                    EnableHandInteractionCallback?.Invoke(false); // <<< Drawフェーズも操作不可
                    TurnManager.StartTurn();
                    RefreshHandDisplayCallback?.Invoke();

                    // --- Tsumo/Kan Check ---
                    // If win/kan happens, skip discard phase logic below

                    // --- Discard Phase ---
                    CurrentPhase = GamePhase.DiscardPhase;
                    Debug.WriteLine($"[Game] Entering Discard Phase for Player {TurnManager.GetCurrentTurnSeat()}");

                    if (TurnManager.IsHumanTurn())
                    {
                        EnableHandInteractionCallback?.Invoke(true); // <<< ★★★ここで操作可能にする★★★
                        await WaitForHumanDiscardAsync();
                        // WaitForHumanDiscardAsyncが完了したら、UIは既に無効化されているはず(NotifyHumanDiscardOfTurnManager内で)
                        Debug.WriteLine($"[Game] Human discard completed.");
                    }
                    else // AI Turn
                    {
                        EnableHandInteractionCallback?.Invoke(false); // <<< AIターン中は操作不可
                        await Task.Delay(10000); // Thinking delay
                        TurnManager.DiscardByAI();
                        RefreshHandDisplayCallback?.Invoke();
                        Debug.WriteLine($"[Game] AI discard completed.");
                    }

                    // --- Call Check Phase ---
                    if (TurnManager.LastDiscardedTile != null && CurrentPhase != GamePhase.RoundOver) // Only check if discard happened and round not ended
                    {
                        CurrentPhase = GamePhase.CallCheckPhase;
                        EnableHandInteractionCallback?.Invoke(false); // <<< コールチェック中は操作不可
                        Debug.WriteLine($"[Game] Entering Call Check Phase for tile: {TurnManager.LastDiscardedTile.Name()}");
                        // await TurnManager.CheckCalls(...); // 鳴き処理待ち
                        // If call happens, TurnManager might change CurrentTurnSeat
                    }

                    // --- Advance Turn ---
                    if (CurrentPhase != GamePhase.RoundOver && CurrentPhase != GamePhase.GameOver)
                    {
                        CurrentPhase = GamePhase.TurnEndPhase;
                        EnableHandInteractionCallback?.Invoke(false); // <<< ターン終了フェーズも操作不可
                        TurnManager.NextTurn();
                        Debug.WriteLine($"[Game] Advancing to next turn: Player {TurnManager.GetCurrentTurnSeat()}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[FATAL ERROR] Exception in StartGame loop: {ex.Message}\n{ex.StackTrace}");
                    CurrentPhase = GamePhase.GameOver;
                    EnableHandInteractionCallback?.Invoke(false); // <<< エラー時も操作不可
                    break;
                }
            }

            // Game loop finished
            EnableHandInteractionCallback?.Invoke(false); // <<< ゲーム終了時も操作不可
            if (CurrentPhase != GamePhase.GameOver)
            {
                CurrentPhase = GamePhase.RoundOver;
                TurnManager.EndRound();
            }
            Debug.WriteLine("[Game] Game loop finished.");
        }

        private TaskCompletionSource<bool>? _humanDiscardTcs;

        private Task WaitForHumanDiscardAsync()
        {
            _humanDiscardTcs = new TaskCompletionSource<bool>();
            Debug.WriteLine("[Game] Waiting for human discard...");
            return _humanDiscardTcs.Task;
        }

        /// <summary>
        /// UIからの人間プレイヤー打牌完了通知を受け取ります。
        /// </summary>
        public void NotifyHumanDiscardOfTurnManager()
        {
            Debug.WriteLine("[Game] Received notification of human discard.");
            // ★★★ 打牌完了したのでUI操作を不可にする ★★★
            EnableHandInteractionCallback?.Invoke(false);
            // Taskを完了させてゲームループを続行
            _humanDiscardTcs?.TrySetResult(true);
        }

        public bool IsHumanTurnFromTurnManager()
        {
            return TurnManager.IsHumanTurn();
        }

        public HumanPlayer? GetHumanPlayer()
        {
            return Players.OfType<HumanPlayer>().FirstOrDefault();
        }

        public void SetUpdateUICallBack(Action refreshHandDisplay)
        {
            RefreshHandDisplayCallback = refreshHandDisplay;
        }

        /// <summary>
        /// UI 操作の有効/無効を切り替えるコールバックを設定します。
        /// </summary>
        public void SetEnableHandInteractionCallback(Action<bool> enableInteraction) // <<< 追加
        {
            EnableHandInteractionCallback = enableInteraction;
        }

        public void Test()
        {
            _ = StartGame();
        }
    }
}