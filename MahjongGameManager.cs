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
        private readonly List<Player> _players;
        private readonly Wall _wall;
        private readonly TurnManager _turnManager;
        private readonly CallManager _callManager;
        private readonly ScoreManager _scoreManager; // ScoreManagerのインスタンスを保持

        public GamePhase CurrentPhase { get; private set; } = GamePhase.InitRound;

        private int _initialDealerSeatIndex; // ゲーム開始時の親の SeatIndex (UI基準: 0-3)
        private List<Wind> _seatWinds;       // 各プレイヤーの現在の自風 (Playersリストのインデックスに対応)
        private Wind _currentWind;           // 場風
        private int _currentRound;           // 現在の局数 (例: 1局目、2局目)
        private int _honba;                  // 本場
        private int _riichiSticks;           // 供託リーチ棒
        private bool _isGameRunning = false;

        // UI更新用コールバック
        private Action? _refreshHandDisplayCallback;
        private Action? _refreshDiscardWallDisplayCallback;
        private Action? _refreshGameCenterDisplaysCallback;
        private Action? _refreshDoraIndicator;
        private Action<bool>? _enableHandInteractionCallback; // 手牌操作の有効/無効

        // 人間プレイヤーの打牌を待つためのTaskCompletionSource
        private TaskCompletionSource<Tile?>? _humanDiscardTcs;

        public MahjongGameManager()
        {
            _wall = new Wall();
            _players = new List<Player>(Config.Instance.NumberOfPlayers);
            _seatWinds = new List<Wind>(Config.Instance.NumberOfPlayers);

            InitializeGameProperties(); // 親決め、場風、局数などの初期化
            InitializePlayers();        // プレイヤーオブジェクトの生成と初期設定

            // GameManagerがScoreManagerのインスタンスを生成・保持
            _scoreManager = new ScoreManager(_players); // ScoreManagerにプレイヤーリストを渡す

            // CallManagerとTurnManagerを初期化（dealerSeatはInitializeGameで設定後に渡す）
            _callManager = new CallManager(_players, 0); // 仮のdealerSeatで初期化
            _turnManager = new TurnManager(_players, _wall, _callManager, 0); // 仮のdealerSeatで初期化
        }

        /// <summary>
        /// UI更新や人間の入力処理のためのコールバックを設定します。
        /// MainFormから呼び出されることを想定しています。
        /// </summary>
        public void InitializeUICallbacks(
            Action refreshHandDisplay,
            Action refreshDiscardWallDisplay,
            Action refreshGameCenterDisplays,
            Action refreshDoraIndicator,
            Action<bool> enableHandInteraction)
        {
            _refreshHandDisplayCallback = refreshHandDisplay;
            _refreshDiscardWallDisplayCallback = refreshDiscardWallDisplay;
            _refreshGameCenterDisplaysCallback = refreshGameCenterDisplays;
            _refreshDoraIndicator = refreshDoraIndicator;
            _enableHandInteractionCallback = enableHandInteraction;

            // TurnManagerにもコールバックを設定
            _turnManager.SetCallbacks(
                RequestHumanDiscardAsync, // 人間の打牌を待つメソッドを渡す
                RefreshAllDisplays // UI全体更新用メソッドを渡す
            );
        }

        /// <summary>
        /// 新しいゲームを開始します。
        /// </summary>
        public async Task StartNewGameAsync()
        {
            if (_isGameRunning)
            {
                Debug.WriteLine("[GameManager] Game is already running.");
                return;
            }
            _isGameRunning = true;
            CurrentPhase = GamePhase.InitRound; // ゲーム開始時は局の初期化から

            Debug.WriteLine("[GameManager] Starting new game...");
            // InitializeGameProperties(); // 親決め、場風、局数などの初期化
            // InitializePlayers();        // プレイヤーオブジェクトの生成と初期設定

            // TurnManagerとCallManagerに正しい最初の親情報を設定
            _turnManager.StartNewRound(_initialDealerSeatIndex); // TurnManagerにも最初の親を通知
            // _callManager.UpdateDealerSeat(_initialDealerSeatIndex); // CallManagerにも更新

            await RunGameLoopAsync();

            _isGameRunning = false;
            Debug.WriteLine("[GameManager] Game has ended.");
            // TODO: ゲーム終了時の最終結果表示などの処理
        }

        /// <summary>
        /// ゲームの基本的なプロパティ（親、場風、局、本場、リーチ棒）を初期化します。
        /// </summary>
        private void InitializeGameProperties()
        {
            _initialDealerSeatIndex = SharedRandom.Instance.Next(Config.Instance.NumberOfPlayers);
            _currentWind = Wind.East;
            _currentRound = 1;
            _honba = 0;
            _riichiSticks = 0;

            Debug.WriteLine($"[GameManager] Initialized game properties. Dealer: {_initialDealerSeatIndex}, Wind: {_currentWind}, Round: {_currentRound}");
        }

        /// <summary>
        /// プレイヤーオブジェクトを生成し、初期設定（席、自風、親フラグ）を行います。
        /// </summary>
        private void InitializePlayers()
        {
            _players.Clear();
            _seatWinds.Clear(); // 先にクリア

            for (int i = 0; i < Config.Instance.NumberOfPlayers; i++)
            {
                Player newPlayer;
                if (i == 0) // 最初のプレイヤーを人間とする
                {
                    newPlayer = new HumanPlayer { SeatIndex = i, Name = $"Player {i} (Human)" };
                }
                else
                {
                    newPlayer = new AIPlayer { SeatIndex = i, Name = $"Player {i} (AI)" };
                }
                newPlayer.Points = Config.Instance.NumberOfPlayers == 4 ? 25000 : 35000; // 初期点数
                _players.Add(newPlayer);
            }

            UpdatePlayerStatesForNewRound(); // 自風と親フラグを更新

            Debug.WriteLine($"[GameManager] Initialized {_players.Count} players.");
        }

        /// <summary>
        /// 新しい局の開始時にプレイヤーの状態（自風、親フラグ、手牌など）を更新します。
        /// </summary>
        private void UpdatePlayerStatesForNewRound()
        {
            // 親の自風は常に東
            Wind dealerActualWind = Wind.East;
            _seatWinds = new List<Wind>(new Wind[Config.Instance.NumberOfPlayers]); // 新しいリストで初期化

            for (int i = 0; i < _players.Count; i++)
            {
                _players[i].IsDealer = (i == _initialDealerSeatIndex);

                // プレイヤーiの自風を計算
                int relativePositionToDealer = (i - _initialDealerSeatIndex + _players.Count) % _players.Count;
                _seatWinds[i] = (Wind)(((int)dealerActualWind + relativePositionToDealer) % _players.Count);

                _players[i].Hand.Clear();
                _players[i].Discards.Clear();
                _players[i].Melds.Clear();
                _players[i].IsRiichi = false;
                _players[i].IsTsumo = false;
                // Debug.WriteLine($"[GameManager] Player {_players[i].SeatIndex} IsDealer: {_players[i].IsDealer}, SeatWind: {_seatWinds[i]}");
            }
        }


        /// <summary>
        /// 配牌処理を行います。
        /// </summary>
        private void DealInitialTiles()
        {
            _wall.InitializeWall(); // 壁牌を初期化・シャッフル

            int initialHandSize = Config.Instance.NumberOfFirstHands;
            foreach (Player player in _players)
            {
                for (int i = 0; i < initialHandSize; i++)
                {
                    Tile? tile = _wall.Draw();
                    if (tile != null)
                    {
                        player.Draw(tile); // Drawメソッド内でIsTsumoはfalseのまま
                    }
                    else
                    {
                        Debug.WriteLine($"[GameManager ERROR] Wall empty during initial draw for player {player.SeatIndex}!");
                        // ここでエラー処理またはゲーム終了を検討
                        CurrentPhase = GamePhase.GameOver;
                        return;
                    }
                }
                player.SortHand();
            }
            Debug.WriteLine("[GameManager] Initial hands dealt.");
            RefreshAllDisplays();
        }


        /// <summary>
        /// ゲームのメインループを実行します。局の進行を管理します。
        /// </summary>
        private async Task RunGameLoopAsync()
        {
            while (CurrentPhase != GamePhase.GameOver)
            {
                CurrentPhase = GamePhase.InitRound;
                PrepareNewRound(); // 配牌など局の準備

                // 1局の進行ループ
                while (CurrentPhase != GamePhase.RoundOver && CurrentPhase != GamePhase.GameOver)
                {
                    if (_wall.Count == 0 && CurrentPhase != GamePhase.WinOrDrawProcessing) // ツモる牌がない場合 (ただし和了処理中などは除く)
                    {
                        Debug.WriteLine("[GameManager] Wall is empty. Proceeding to round end (exhaustive draw).");
                        CurrentPhase = GamePhase.RoundOver; // 荒牌流局として局終了フェーズへ
                        // ProcessRoundEnd内で流局処理を行う
                        break;
                    }

                    TurnResult turnResult = await _turnManager.ProcessCurrentTurnAsync();
                    CurrentPhase = GamePhase.WinOrDrawProcessing; // ターン結果の処理中フェーズ

                    switch (turnResult.ResultType)
                    {
                        case TurnActionResult.Continue:
                            CurrentPhase = GamePhase.TurnEndPhase; // 次のターンの準備へ
                            break;
                        case TurnActionResult.Win:
                            Debug.WriteLine($"[GameManager] Player {turnResult.WinningPlayer?.Name} won with tile {turnResult.WinningTile?.Name()}.");
                            // TODO: ScoreManagerを使って点数計算し、プレイヤーの点数を更新
                            // _scoreManager.ProcessWin(turnResult.WinningPlayer, turnResult.WinningTile, turnResult.IsSelfWin, _honba, _riichiSticks, _currentWind, _seatWinds[turnResult.WinningPlayer.SeatIndex]);
                            _riichiSticks = 0; // 和了したので供託リーチ棒は回収
                            CurrentPhase = GamePhase.RoundOver;
                            break;
                        case TurnActionResult.ExhaustiveDraw:
                            Debug.WriteLine("[GameManager] Exhaustive draw (wall empty).");
                            CurrentPhase = GamePhase.RoundOver;
                            break;
                        case TurnActionResult.MeldAndContinue:
                            Debug.WriteLine($"[GameManager] Player {turnResult.MeldPlayer?.Name} melded. Turn continues for this player.");
                            // TurnManagerが内部でCurrentTurnSeatを更新しているはず
                            // Meld後の打牌処理をTurnManagerに再度依頼
                            // ここでは、TurnManager.ProcessTurnAfterMeldAsyncを呼び出すか、
                            // あるいはTurnManagerが次の打牌までをMeldAndContinueの結果として処理し終えている前提。
                            // 今回のTurnManagerの実装では、Meld後の打牌もProcessCurrentTurnAsyncやProcessTurnAfterMeldAsyncで処理される想定。
                            // そのため、GameManagerは次のTurnManager.ProcessCurrentTurnAsync()の呼び出しで対応。
                            CurrentPhase = GamePhase.TurnEndPhase; // 実際には同じプレイヤーの打牌フェーズへ
                            break;
                        case TurnActionResult.Error:
                            Debug.WriteLine("[GameManager ERROR] Error occurred in turn processing.");
                            CurrentPhase = GamePhase.GameOver; // エラー発生時はゲーム終了
                            break;
                        default:
                            CurrentPhase = GamePhase.TurnEndPhase;
                            break;
                    }
                    RefreshAllDisplays(); // 各アクション後にUIを更新
                }

                // 1局終了時の処理
                if (CurrentPhase == GamePhase.RoundOver)
                {
                    ProcessRoundEnd(); // 親流れ、連荘、本場、供託の更新など
                    if (IsGameOver())
                    {
                        CurrentPhase = GamePhase.GameOver;
                    }
                }
                RefreshAllDisplays(); // 局終了後にもUI更新
            }
        }

        /// <summary>
        /// 新しい局の準備を行います（配牌、UIリフレッシュなど）。
        /// </summary>
        private void PrepareNewRound()
        {
            Debug.WriteLine($"[GameManager] Preparing new round: {_currentWind} {_currentRound}局 {_honba}本場, Riichi sticks: {_riichiSticks}, Dealer: {_initialDealerSeatIndex}");
            UpdatePlayerStatesForNewRound(); // プレイヤーの自風、親フラグ、手牌などをリセット
            DealInitialTiles();             // 配牌
            _turnManager.StartNewRound(_initialDealerSeatIndex); // TurnManagerに新しい局の開始と親を通知
            // _callManager.UpdateDealerSeat(_initialDealerSeatIndex);

            _enableHandInteractionCallback?.Invoke(false); // 最初は手牌操作不可
            RefreshAllDisplays();
            CurrentPhase = GamePhase.TurnEndPhase; // 最初のターンの準備完了状態
        }


        /// <summary>
        /// 局終了時の処理（親流れ、連荘、本場・リーチ棒の更新など）を行います。
        /// </summary>
        private void ProcessRoundEnd()
        {
            Debug.WriteLine($"[GameManager] Processing Round End. Current Dealer: {_initialDealerSeatIndex} ({_players[_initialDealerSeatIndex].Name})");

            // TODO: 和了処理や流局処理の結果に基づいて、親が連荘するか流れるかを決定する。
            //       ScoreManagerから情報を取得し、判定する。
            //       例: bool dealerWonOrTenpai = _scoreManager.DidDealerWinOrTenpaiOnDraw(_initialDealerSeatIndex);
            bool dealerWonOrTenpai = false; // 仮: 親は和了も聴牌もしていないとする
            // if (和了者 == 親) dealerWonOrTenpai = true;
            // if (流局 && 親が聴牌) dealerWonOrTenpai = true;

            if (dealerWonOrTenpai)
            {
                Debug.WriteLine($"[GameManager] Dealer ({_players[_initialDealerSeatIndex].Name}) won or was tenpai on draw. Renchan (連荘).");
                _honba++; // 連荘なので本場を1増やす
                // 親は変わらない (_initialDealerSeatIndex も _currentRound も変わらない)
            }
            else
            {
                Debug.WriteLine($"[GameManager] Dealer ({_players[_initialDealerSeatIndex].Name}) lost or was noten on draw. Dealer moves.");
                _honba = 0; // 親流れなので本場は0に戻る
                _initialDealerSeatIndex = (_initialDealerSeatIndex + 1) % _players.Count; // 次の人が親
                _currentRound++; // 局が進む

                // 場風が変わるかチェック (東4局が終わったら南1局へ、など)
                if (_currentRound > _players.Count) // 東/南場の1巡が終わった場合
                {
                    if (_currentWind == Wind.East) // 東場が終わった
                    {
                        // 南入するかどうか (東風戦ならここでゲーム終了の可能性)
                        // 今回は東風戦想定として、ここではゲーム終了条件には含めず、IsGameOverで判定
                        // _currentWind = Wind.South;
                        // _currentRound = 1;
                        Debug.WriteLine("[GameManager] East round cycle completed.");
                    }
                    // else if (_currentWind == Wind.South) { /* 西入またはゲーム終了 */ }
                }
            }
            // リーチ棒は、和了があればその局で処理済み、流局なら持ち越し
            // _riichiSticks は ScoreManager が管理するか、ここで増減させる。流局時は増えることはない。

            Debug.WriteLine($"[GameManager] Next round state: Dealer: {_initialDealerSeatIndex}, Wind: {_currentWind}, Round: {_currentRound}, Honba: {_honba}");
        }


        /// <summary>
        /// ゲームの終了条件を判定します。
        /// </summary>
        private bool IsGameOver()
        {
            // 東風戦の終了条件の例:
            // 1. 東4局が終了し、親が流れた場合 (次の局が名目上「東5局」になる場合)
            if (_currentWind == Wind.East && _currentRound > Config.Instance.NumberOfPlayers)
            {
                Debug.WriteLine($"[GameManager] Game Over: East round cycle completed (CurrentRound: {_currentRound}).");
                return true;
            }

            // 2. 誰かの持ち点が0点以下になった場合 (トビ終了)
            if (_players.Any(p => p.Points <= 0))
            {
                Player? bankruptPlayer = _players.FirstOrDefault(p => p.Points <= 0);
                Debug.WriteLine($"[GameManager] Game Over: Player {bankruptPlayer?.Name} has {bankruptPlayer?.Points} points (Tobi).");
                return true;
            }
            // TODO: その他の終了条件（例：時間制限、西入しない設定で南4局終了など）
            return false;
        }

        /// <summary>
        /// 人間プレイヤーに打牌を要求し、その結果を待ちます。
        /// TurnManagerから呼び出されるコールバックとして設定されます。
        /// </summary>
        private async Task<Tile?> RequestHumanDiscardAsync()
        {
            _enableHandInteractionCallback?.Invoke(true); // 手牌操作を有効にする
            _humanDiscardTcs = new TaskCompletionSource<Tile?>();
            Debug.WriteLine("[GameManager] Waiting for human discard via UI...");

            Tile? discardedTile = await _humanDiscardTcs.Task; // MainFormからの通知を待つ

            _enableHandInteractionCallback?.Invoke(false); // 手牌操作を無効にする
            return discardedTile;
        }

        /// <summary>
        /// MainForm（UI）から人間プレイヤーの打牌が完了したことを通知されます。
        /// </summary>
        /// <param name="discardedTile">捨てられた牌。UI側でPlayerモデルの手牌からの削除とDiscardsへの追加は完了している想定。</param>
        public void NotifyHumanDiscard(Tile discardedTile)
        {
            Debug.WriteLine($"[GameManager] Human discard notified: {discardedTile.Name()}");
            // ここでPlayerモデルの更新はUI側で行われている前提。
            // _players[0].DiscardTile(discardedTile); // もしUI側でモデル更新してなければここで行う
            _humanDiscardTcs?.TrySetResult(discardedTile);
        }


        // --- UI 更新用メソッド ---
        private void RefreshAllDisplays()
        {
            _refreshHandDisplayCallback?.Invoke();
            _refreshDiscardWallDisplayCallback?.Invoke();
            _refreshGameCenterDisplaysCallback?.Invoke();
            _refreshDoraIndicator?.Invoke();
        }

        // --- 各DisplayManagerへの情報提供用ゲッター ---
        public HumanPlayer? GetHumanPlayer() => _players.FirstOrDefault(p => p.IsHuman) as HumanPlayer;
        public List<Player> GetPlayers() => _players;
        public int GetHonba() => _honba;
        public int GetRiichiStickCount() => _riichiSticks;
        public List<Wind> GetSeatWinds() => _seatWinds;
        public int GetDealerSeat() => _initialDealerSeatIndex; // 現在の親の席
        public Wind GetCurrentWind() => _currentWind;
        public int GetCurrentRound() => _currentRound;
        public Wall GetWall() => _wall;
        public int GetRemainingTileCount() => _wall.Count;

        /// <summary>
        /// テストやUIからのゲーム開始トリガー用。
        /// </summary>
        public void TriggerStartGameForTest()
        {
            _ = StartNewGameAsync();
        }
    }
}