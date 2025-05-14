// MahjongGameManager.cs
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
        Wall Wall;
        TurnManager TurnManager;
        CallManager CallManager;
        ScoreManager ScoreManager;
        public GamePhase CurrentPhase = GamePhase.InitRound;

        // int RoundNumber;
        // int HonbaCount;
        // int RiichiSticks;
        int DealerIndex;
        List<Wind> SeatWinds;
        Wind CurrentWind;
        int CurrentRound;

        // Callback for UI updates
        Action? RefreshHandDisplayCallback;
        Action? RefreshDiscardWallDisplayCallback;
        Action? RefreshGameCenterDisplays;
        // Callback for enabling/disabling UI hand interaction
        Action<bool>? EnableHandInteractionCallback; // <<< 追加

        public MahjongGameManager()
        {
            Wall = new Wall();
            Players = new List<Player>();
            InitializeGame();
            CallManager = new CallManager(Players, DealerIndex);
            ScoreManager = new ScoreManager();
            TurnManager = new TurnManager(Players, Wall, CallManager, ScoreManager, DealerIndex);
            TurnManager.SetUpdateUICallBack(() => RefreshHandDisplayCallback?.Invoke());
        }

        void InitializeGame()
        {
            DealerIndex = 0;
            CurrentWind = Wind.East;
            SeatWinds = new List<Wind>(Config.Instance.NumberOfPlayers);
            int numPlayers = Config.Instance.NumberOfPlayers;
            Wind humanWind = Wind.East;
            if (Players == null) Players = new List<Player>(numPlayers);
            Players.Clear();
            for (int i = 0; i < numPlayers; i++)
            {
                Player newPlayer;
                if (i == 0)
                {
                    newPlayer = new HumanPlayer { SeatIndex = i, Name = $"Player {i} (Human)" };
                }
                else { newPlayer = new AIPlayer { SeatIndex = i, Name = $"Player {i} (AI)" }; }
                newPlayer.IsDealer = (i == DealerIndex);
                Players.Add(newPlayer);
            }

            for (int i = 0; i < numPlayers; i++) { SeatWinds.Add((Wind)(((int)humanWind + i) % numPlayers)); }

            Debug.WriteLine($"[Game] Initialized {Players.Count} players. Dealer is Player {DealerIndex}.");
        }

        public async Task StartGame()
        {
            Debug.WriteLine("[Game] Starting game...");
            CurrentRound = 1;

            CurrentPhase = GamePhase.InitRound;
            TurnManager.StartNewRound();
            EnableHandInteractionCallback?.Invoke(false); // <<< 初期状態は操作不可
            RefreshHandDisplayCallback?.Invoke();
            RefreshGameCenterDisplays?.Invoke();

            while (Wall.Count > 0 && CurrentPhase != GamePhase.GameOver)
            {
                try
                {
                    Debug.WriteLine($"[Game] ----- Turn Start: Player {TurnManager.GetCurrentTurnSeat()}, Wall: {Wall.Count} -----");
                    CurrentPhase = GamePhase.DrawPhase;
                    EnableHandInteractionCallback?.Invoke(false); // <<< Drawフェーズも操作不可
                    TurnManager.StartTurn();
                    RefreshHandDisplayCallback?.Invoke();
                    RefreshGameCenterDisplays?.Invoke();

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
                        // await Task.Delay(1000); // Thinking delay
                        TurnManager.DiscardByAI();
                        // RefreshHandDisplayCallback?.Invoke();
                        RefreshDiscardWallDisplayCallback?.Invoke();
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
        public List<Player>? GetPlayers()
        {
            return Players;
        }

        public int GetHonba()
        {
            return 0; // 仮: this.HonbaCount; を返す (HonbaCountの管理ロジックが必要)
        }

        public int GetRiichiStickCount()
        {
            return 0; // 仮: this.RiichiSticks; を返す (RiichiSticksの管理ロジックが必要)
        }

        public int GetCurrentTurnSeat() { return TurnManager.GetCurrentTurnSeat(); }

        public List<Wind> GetSeatWinds() { return SeatWinds; }
        public int GetDealerSeat() { return TurnManager.GetDealerSeat(); }
        public Wind GetCurrentWind() { return CurrentWind; }
        public int GetCurrentRound() { return CurrentRound; }

        public int GetRemainingTileCount() { return Wall.Count; }

        public void SetUpdateUICallBack(Action refreshHandDisplay, Action refreshDiscardWallDisplay, Action refreshGameCenterDisplays)
        {
            RefreshHandDisplayCallback = refreshHandDisplay;
            RefreshDiscardWallDisplayCallback = refreshDiscardWallDisplay;
            RefreshGameCenterDisplays = refreshGameCenterDisplays;
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