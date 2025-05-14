// MahjongGameManager.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MahjongApp
{
    public class MahjongGameManager
    {
        List<Player> Players; // インデックス 0:自家, 1:下家, 2:対面, 3:上家
        Wall Wall;
        TurnManager TurnManager;
        CallManager CallManager;
        ScoreManager ScoreManager;
        public GamePhase CurrentPhase = GamePhase.InitRound;

        int InitialDealerSeatIndex; // ゲーム開始時の親の SeatIndex (UI基準: 0-3)
        List<Wind> SeatWinds;       // 各プレイヤーの現在の自風 (Playersリストのインデックスに対応)
        Wind CurrentWind;           // 場風
        int CurrentRound;           // 現在の局数 (例: 1局目、2局目)

        Action? RefreshHandDisplayCallback;
        Action? RefreshDiscardWallDisplayCallback;
        Action? RefreshGameCenterDisplays;
        Action<bool>? EnableHandInteractionCallback;

        public MahjongGameManager()
        {
            Wall = new Wall();
            Players = new List<Player>(Config.Instance.NumberOfPlayers); // Initialize with capacity
            SeatWinds = new List<Wind>(Config.Instance.NumberOfPlayers);
            InitializeGame(); // Players と SeatWinds を設定

            // DealerIndex (TurnManagerに渡す親) は InitialDealerSeatIndex を使う
            CallManager = new CallManager(Players, InitialDealerSeatIndex);
            ScoreManager = new ScoreManager(); // Players を渡す必要があれば修正
            TurnManager = new TurnManager(Players, Wall, CallManager, ScoreManager, InitialDealerSeatIndex);
            TurnManager.SetUpdateUICallBack(() => RefreshHandDisplayCallback?.Invoke());
        }

        void InitializeGame()
        {
            int numPlayers = Config.Instance.NumberOfPlayers;
            Players.Clear();
            SeatWinds.Clear(); // SeatWindsもクリア

            // 仮に人間プレイヤー(自家)を常に SeatIndex 0 とし、東家スタートとする
            // TODO: 親決めロジックをここに入れる (例: サイコロを振るなど)
            // 現状は、人間プレイヤーが常に最初の親(東家)と仮定
            InitialDealerSeatIndex = SharedRandom.Instance.Next(numPlayers);
            CurrentWind = Wind.East;    // 初期場風は東
            CurrentRound = 1;           // 初期局数は1

            // プレイヤーインスタンスの生成 (UI視点: 0=自家, 1=下家, 2=対面, 3=上家)
            for (int i = 0; i < numPlayers; i++)
            {
                Player newPlayer;
                if (i == 0) // 自家 (人間プレイヤー)
                {
                    newPlayer = new HumanPlayer { SeatIndex = i, Name = $"Player {i} (Human)" };
                }
                else // 下家、対面、上家 (AIプレイヤー)
                {
                    newPlayer = new AIPlayer { SeatIndex = i, Name = $"Player {i} (AI)" };
                }
                Players.Add(newPlayer);
            }

            // 各プレイヤーの初期自風と親フラグを設定
            // 起家(最初の親)を基準に東南西北を割り振る
            Wind dealerActualWind = Wind.East; // 起家は必ず東
            for (int i = 0; i < numPlayers; i++)
            {
                // i は UI上の席順 (0=自家, 1=下家, 2=対面, 3=上家)
                // InitialDealerSeatIndex は UI上の席順での親の位置
                // プレイヤーiの自風を計算
                // (i - InitialDealerSeatIndex + numPlayers) % numPlayers で親から見た相対位置が出る
                // 0: 親自身, 1: 親の下家, 2: 親の対面, 3: 親の上家
                int relativePositionToDealer = (i - InitialDealerSeatIndex + numPlayers) % numPlayers;
                Wind playerSeatWind = (Wind)(((int)dealerActualWind + relativePositionToDealer) % numPlayers);
                SeatWinds.Add(playerSeatWind); // SeatWinds は Players のインデックスに対応

                Players[i].IsDealer = (i == InitialDealerSeatIndex); // 親フラグ設定
            }

            Debug.WriteLine($"[Game] Initialized {Players.Count} players.");
            Debug.WriteLine($"[Game] Initial Dealer (UI SeatIndex): {InitialDealerSeatIndex}, Their Wind: {SeatWinds[InitialDealerSeatIndex]}");
            for (int i = 0; i < numPlayers; i++)
            {
                Debug.WriteLine($"[Game] Player {Players[i].SeatIndex} (UI) is {SeatWinds[i]}");
            }
        }

        public async Task StartGame()
        {
            Debug.WriteLine("[Game] Starting game...");
            // CurrentRound は InitializeGame で設定済み

            CurrentPhase = GamePhase.InitRound;
            TurnManager.StartNewRound(); // TurnManagerは渡されたDealerSeatIndexで開始
            EnableHandInteractionCallback?.Invoke(false);
            RefreshHandDisplayCallback?.Invoke();
            RefreshGameCenterDisplays?.Invoke(); // SeatWinds と DealerSeat をUIに反映

            while (Wall.Count > 0 && CurrentPhase != GamePhase.GameOver)
            {
                try
                {
                    int currentTurnPlayerSeatIndex = TurnManager.GetCurrentTurnSeat(); // UI基準のSeatIndex
                    Debug.WriteLine($"[Game] ----- Turn Start: Player {currentTurnPlayerSeatIndex} ({SeatWinds[currentTurnPlayerSeatIndex]}), Wall: {Wall.Count} -----");
                    CurrentPhase = GamePhase.DrawPhase;
                    EnableHandInteractionCallback?.Invoke(false);
                    TurnManager.StartTurn();
                    RefreshHandDisplayCallback?.Invoke();
                    RefreshGameCenterDisplays?.Invoke();

                    // ... (以降のゲームループはCurrentTurnSeatがUI基準のインデックスであることを前提に動作するはず) ...

                    CurrentPhase = GamePhase.DiscardPhase;
                    Debug.WriteLine($"[Game] Entering Discard Phase for Player {currentTurnPlayerSeatIndex}");

                    if (TurnManager.IsHumanTurn()) // IsHumanTurnは現在のPlayers[CurrentTurnSeat]がHumanかで判定
                    {
                        EnableHandInteractionCallback?.Invoke(true);
                        await WaitForHumanDiscardAsync();
                        Debug.WriteLine($"[Game] Human discard completed.");
                    }
                    else // AI Turn
                    {
                        EnableHandInteractionCallback?.Invoke(false);
                        await Task.Delay(1000); // AIの思考時間を模擬 (実際のAIロジックはここに入る)
                        TurnManager.DiscardByAI();
                        RefreshDiscardWallDisplayCallback?.Invoke();
                        Debug.WriteLine($"[Game] AI discard completed.");
                    }

                    if (TurnManager.LastDiscardedTile != null && CurrentPhase != GamePhase.RoundOver)
                    {
                        CurrentPhase = GamePhase.CallCheckPhase;
                        EnableHandInteractionCallback?.Invoke(false);
                        Debug.WriteLine($"[Game] Entering Call Check Phase for tile: {TurnManager.LastDiscardedTile.Name()}");
                        // CallManagerの処理もUI基準のSeatIndexで行われる想定
                    }

                    if (CurrentPhase != GamePhase.RoundOver && CurrentPhase != GamePhase.GameOver)
                    {
                        CurrentPhase = GamePhase.TurnEndPhase;
                        EnableHandInteractionCallback?.Invoke(false);
                        TurnManager.NextTurn(); // TurnManager内部でCurrentTurnSeatが(CurrentTurnSeat + 1) % numPlayersされる
                        Debug.WriteLine($"[Game] Advancing to next turn: Player {TurnManager.GetCurrentTurnSeat()}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[FATAL ERROR] Exception in StartGame loop: {ex.Message}\n{ex.StackTrace}");
                    CurrentPhase = GamePhase.GameOver;
                    EnableHandInteractionCallback?.Invoke(false);
                    break;
                }
            }

            EnableHandInteractionCallback?.Invoke(false);
            if (CurrentPhase != GamePhase.GameOver)
            {
                CurrentPhase = GamePhase.RoundOver;
                TurnManager.EndRound(); // TODO: EndRound内で親流れ、連荘、局と場風の更新
                                        // UpdateRoundAndDealer(); // 新しいメソッド例
            }
            Debug.WriteLine("[Game] Game loop finished.");
        }

        // 仮: UpdateRoundAndDealer メソッド (EndRoundから呼ばれる想定)
        // ここで親が流れたか、連荘かなどを判断し、
        // 次の局の DealerIndex (UI基準), CurrentWind, CurrentRound, SeatWinds を更新する
        /*
        void UpdateRoundAndDealer()
        {
            bool dealerWonOrTenpai = ...; // 親が和了ったかテンパイだったか
            bool allPlayersTenpaiInDraw = ...; // 流局時に全員テンパイだったか

            if (dealerWonOrTenpai || allPlayersTenpaiInDraw) // 連荘条件
            {
                // DealerIndex は変わらない
                // 本場を増やすなど
            }
            else // 親流れ
            {
                InitialDealerSeatIndex = (InitialDealerSeatIndex + 1) % Config.Instance.NumberOfPlayers; // 次の人が親
                // PlayersリストのIsDealerフラグを更新
                for(int i=0; i < Players.Count; i++) Players[i].IsDealer = (i == InitialDealerSeatIndex);

                // 場風が変わるかチェック (東場 -> 南場など)
                // 例: (CurrentRound / Config.Instance.NumberOfPlayers) の商が変わったら場風も進める
                // CurrentRound++;
                // if ((CurrentRound -1) % Config.Instance.NumberOfPlayers == 0 && CurrentRound > 1) // 4局ごとに場風が変わる場合
                // {
                //    CurrentWind = (Wind)((int)CurrentWind + 1);
                //    if ((int)CurrentWind >= Config.Instance.NumberOfPlayers) CurrentWind = Wind.East; // 例: 北場の次はない想定
                // }
            }
            // 新しいDealerIndexとCurrentWindに基づいて、全プレイヤーのSeatWindsを再計算
            Wind newDealerActualWind = Wind.East; // 親は常に自風が東として扱われる
            for (int i = 0; i < Players.Count; i++)
            {
                int relativePositionToNewDealer = (i - InitialDealerSeatIndex + Players.Count) % Players.Count;
                SeatWinds[i] = (Wind)(((int)newDealerActualWind + relativePositionToNewDealer) % Players.Count);
            }

            TurnManager.SetDealerSeat(InitialDealerSeatIndex); // TurnManagerにも新しい親を通知
            RefreshGameCenterDisplays?.Invoke(); // UI更新
        }
        */


        private TaskCompletionSource<bool>? _humanDiscardTcs;

        private Task WaitForHumanDiscardAsync()
        {
            _humanDiscardTcs = new TaskCompletionSource<bool>();
            Debug.WriteLine("[Game] Waiting for human discard...");
            return _humanDiscardTcs.Task;
        }

        public void NotifyHumanDiscardOfTurnManager()
        {
            Debug.WriteLine("[Game] Received notification of human discard.");
            EnableHandInteractionCallback?.Invoke(false);
            _humanDiscardTcs?.TrySetResult(true);
        }

        public bool IsHumanTurnFromTurnManager()
        {
            return TurnManager.IsHumanTurn();
        }

        public HumanPlayer? GetHumanPlayer()
        {
            // Players[0] が人間プレイヤーであるという前提
            return Players[0] as HumanPlayer;
        }
        public List<Player>? GetPlayers()
        {
            // このPlayersリストはUI視点の並びになっている
            return Players;
        }

        public int GetHonba()
        {
            return 0;
        }

        public int GetRiichiStickCount()
        {
            return 0;
        }

        public int GetCurrentTurnSeat() { return TurnManager.GetCurrentTurnSeat(); } // UI基準のSeatIndex

        // SeatWinds はUI基準のプレイヤーの自風リスト (Players[i] の自風が SeatWinds[i])
        public List<Wind> GetSeatWinds() { return SeatWinds; }

        public int GetDealerSeat() { return TurnManager.GetDealerSeat(); } // UI基準のSeatIndex

        public Wind GetCurrentWind() { return CurrentWind; } // 場風
        public int GetCurrentRound() { return CurrentRound; } // 局数 (1から始まる)

        public int GetRemainingTileCount() { return Wall.Count; }

        public void SetUpdateUICallBack(Action refreshHandDisplay, Action refreshDiscardWallDisplay, Action refreshGameCenterDisplays)
        {
            RefreshHandDisplayCallback = refreshHandDisplay;
            RefreshDiscardWallDisplayCallback = refreshDiscardWallDisplay;
            RefreshGameCenterDisplays = refreshGameCenterDisplays;
        }

        public void SetEnableHandInteractionCallback(Action<bool> enableInteraction)
        {
            EnableHandInteractionCallback = enableInteraction;
        }

        public void Test()
        {
            _ = StartGame();
        }
    }
}