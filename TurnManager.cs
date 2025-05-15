// TurnManager.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq; // Add this for LINQ methods like LastOrDefault
using System.Threading.Tasks;

namespace MahjongApp
{
    public class TurnManager
    {
        private int _currentTurnSeat; // UI基準の席インデックス (0:自分, 1:右, 2:対面, 3:左)
        private int _dealerSeat;
        private readonly List<Player> _players;
        private readonly Wall _wall;
        private readonly CallManager _callManager;
        // private readonly ScoreManager _scoreManager; // 点数計算はGameManager経由の想定

        public Tile? LastDiscardedTile { get; private set; }
        private Tile? _lastDrawnTile; // 現在のターンプレイヤーがツモった牌

        // GameManagerから設定されるコールバック
        private Func<Task<Tile?>>? _requestHumanDiscardCallback; // 人間プレイヤーの打牌を要求・待機する
        private Action? _refreshUIDisplayCallback; // UI更新全般
        private Action<Player, Tile, bool>? _processWinCallback; // 和了処理をGameManagerに依頼 (winner, winningTile, isTsumo)
        private Action<Player, Meld>? _processMeldCallback; // 鳴き処理をGameManagerに依頼

        public TurnManager(List<Player> players, Wall wall, CallManager callManager, int dealerSeat)
        {
            _players = players;
            _wall = wall;
            _callManager = callManager;
            _dealerSeat = dealerSeat;
            _currentTurnSeat = dealerSeat; // 親からスタート
        }

        public void SetCallbacks(
            Func<Task<Tile?>>? requestHumanDiscardCallback,
            Action? refreshUIDisplayCallback,
            Action<Player, Tile, bool>? processWinCallback = null, // 必要に応じて追加
            Action<Player, Meld>? processMeldCallback = null)      // 必要に応じて追加
        {
            _requestHumanDiscardCallback = requestHumanDiscardCallback;
            _refreshUIDisplayCallback = refreshUIDisplayCallback;
            _processWinCallback = processWinCallback;
            _processMeldCallback = processMeldCallback;
        }

        public void StartNewRound(int dealerSeat)
        {
            Debug.WriteLine($"[TurnManager] Starting New Round. Dealer: {dealerSeat}");
            _dealerSeat = dealerSeat;
            _currentTurnSeat = _dealerSeat;
            LastDiscardedTile = null;
            _lastDrawnTile = null;

            // 壁牌の初期化と配牌はGameManagerの責務とし、ここでは行わない。
            // GameManagerが配牌後、最初のターンのためにTurnManagerを呼び出す。
        }

        public int GetCurrentTurnSeat() => _currentTurnSeat;
        public Player GetCurrentPlayer() => _players[_currentTurnSeat];

        /// <summary>
        /// 1プレイヤーのターン全体を処理します。
        /// ツモ、自己アクション（カン、ツモ和了）、打牌、他家のアクション（鳴き、ロン）を含みます。
        /// </summary>
        public async Task<TurnResult> ProcessCurrentTurnAsync()
        {
            Player currentPlayer = GetCurrentPlayer();
            Debug.WriteLine($"[TurnManager] ----- Turn Start: Player {_currentTurnSeat} ({currentPlayer.Name}), Wall: {_wall.Count} -----");

            // 0. 壁牌がなければ流局
            if (_wall.Count == 0)
            {
                Debug.WriteLine("[TurnManager] Wall is empty at turn start. Exhaustive Draw.");
                return TurnResult.CreateExhaustiveDraw();
            }

            // 1. ツモ
            _lastDrawnTile = _wall.Draw();
            if (_lastDrawnTile == null) // 通常は起こり得ないが念のため
            {
                Debug.WriteLine("[TurnManager ERROR] Failed to draw tile from non-empty wall.");
                return TurnResult.CreateError();
            }
            currentPlayer.Draw(_lastDrawnTile); // 手牌に加え、IsTsumoフラグを立てる
            Debug.WriteLine($"[TurnManager] Player {_currentTurnSeat} drew: {_lastDrawnTile.Name()}");
            _refreshUIDisplayCallback?.Invoke();

            // 2. ツモ後の自己アクションチェック
            // TODO: ここでツモ和了、暗槓、加槓のチェックを行う
            // 例: if (YakuChecker.CanWin(currentPlayer.Hand, _lastDrawnTile, isTsumo: true, ...)) {
            //      return TurnResult.CreateWin(currentPlayer, _lastDrawnTile, isSelfWin: true);
            //    }
            // 例: if (CanDeclareAnkan(currentPlayer, _lastDrawnTile)) { /* カン宣言処理へ */ }
            // カン宣言した場合、嶺上開花やドラめくりなどが発生し、再度打牌へ。
            // この部分は複雑なので、一旦スキップして打牌へ進みます。

            // 3. 打牌
            Tile? tileToDiscard;
            if (currentPlayer.IsHuman)
            {
                if (_requestHumanDiscardCallback == null)
                {
                    Debug.WriteLine("[TurnManager ERROR] Human discard callback is not set.");
                    return TurnResult.CreateError();
                }
                Debug.WriteLine($"[TurnManager] Requesting discard from Human Player {_currentTurnSeat}");
                tileToDiscard = await _requestHumanDiscardCallback.Invoke(); // GameManager経由でUIからの入力を待つ

                if (tileToDiscard == null)
                {
                    // 人間が打牌しなかった場合（時間切れでツモ切りなど、GameManager側でハンドリングされる想定）
                    // または、不正な操作でnullが来た場合。
                    // ここでは、エラーとして扱うか、強制的にツモ切り牌(_lastDrawnTile)を捨てる。
                    // GameManagerがnullを返さない前提であれば、ここはエラーチェックのみ。
                    Debug.WriteLine($"[TurnManager ERROR] Human Player {_currentTurnSeat} did not provide a tile to discard.");
                    // 強制ツモ切りする場合:
                    // tileToDiscard = _lastDrawnTile;
                    // if (!currentPlayer.Hand.Contains(tileToDiscard)) // _lastDrawnTileが既に手牌の一部として処理されている場合を考慮
                    // {
                    //    tileToDiscard = currentPlayer.Hand.LastOrDefault(t => t.Equals(_lastDrawnTile));
                    // }
                    // if (tileToDiscard == null && currentPlayer.Hand.Any()) tileToDiscard = currentPlayer.Hand.Last();

                    // ここでは、GameManagerが必ず有効な牌か、ツモ切り牌を返すことを期待する。
                    // もしnullが来た場合は、GameManager側でエラー処理か、局終了の指示があったと見なす。
                    return TurnResult.CreateError(); // または適切なエラー処理
                }
                // GameManagerが選択された牌をPlayerモデルから削除し、Discardsに追加する処理は、
                // GameManagerのコールバック内で行われている想定。
                // TurnManagerは、どの牌が捨てられたかを知るだけ。
                // currentPlayer.DiscardTile(tileToDiscard); // これはGameManager側で行うべき
                // LastDiscardedTile の設定は、GameManagerが打牌を確定させた後に行うべき。
                // この設計だと、TurnManagerがLastDiscardedTileを設定するタイミングが難しい。
                // GameManagerが打牌を確定させ、その牌をTurnManagerに通知する形が良い。
                // → GameManagerが Player.DiscardTile() を呼び、その後 TurnManager に LastDiscardedTile をセットし、
                //    他家のアクションチェックを依頼する、という流れにする。
                //    つまり、このメソッドは打牌選択まで。
                //    今回は、このメソッド内で打牌まで行う設計とする。
                //    GameManagerは`_requestHumanDiscardCallback`内でPlayerモデルの更新も行う。
            }
            else // AIプレイヤー
            {
                await Task.Delay(Config.Instance.AiThinkTimeMs); // AIの思考時間
                tileToDiscard = currentPlayer.ChooseDiscardTile();
                if (tileToDiscard == null)
                {
                    Debug.WriteLine($"[TurnManager ERROR] AI Player {_currentTurnSeat} failed to choose a discard tile. Discarding last drawn.");
                    tileToDiscard = _lastDrawnTile; // ツモ切り
                }
                // AIが選択した牌を実際に捨てる (Playerモデルの更新)
                if (!currentPlayer.Hand.Contains(tileToDiscard) && tileToDiscard.Equals(_lastDrawnTile))
                {
                    //ツモ切りで、すでにDraw()でHandに入っている_lastDrawnTileそのものを指定した場合
                }
                else if (!currentPlayer.Hand.Contains(tileToDiscard))
                {
                    Debug.WriteLine($"[TurnManager ERROR] AI chose to discard tile not in hand: {tileToDiscard.Name()}. Forcing discard of last drawn.");
                    tileToDiscard = _lastDrawnTile;
                }
                currentPlayer.DiscardTile(tileToDiscard); // Playerの手牌から削除し、捨て牌リストに追加
            }

            LastDiscardedTile = tileToDiscard; // 実際に捨てられた牌を記録
            currentPlayer.IsTsumo = false; // 打牌したのでツモ状態は解除
            Debug.WriteLine($"[TurnManager] Player {_currentTurnSeat} discarded: {LastDiscardedTile.Name()}");
            _refreshUIDisplayCallback?.Invoke();

            // 4. 打牌後の他家のアクションチェック
            // TODO: CallManagerを使用して、ロン、ポン、チー、カンをチェックする
            // 例: var callResponses = _callManager.CheckCalls(LastDiscardedTile, _currentTurnSeat, _players);
            //    if (callResponses.Any(r => r.Type == CallType.Ron)) {
            //        Player winner = callResponses.First(r => r.Type == CallType.Ron).Player;
            //        return TurnResult.CreateWin(winner, LastDiscardedTile, isSelfWin: false);
            //    }
            //    if (callResponses.Any(r => r.Type == CallType.Pon || r.Type == CallType.Kan || r.Type == CallType.Chi)) {
            //        // 鳴き処理を実行し、鳴いたプレイヤーにターンを移す
            //        // Meld meldAction = ExecuteMeld(...)
            //        // _currentTurnSeat = meldAction.Player.SeatIndex;
            //        // return TurnResult.CreateMeldAndContinue(meldAction.Player, meldAction);
            //    }
            // CallManagerの具体的な実装がないため、ここは仮のロジックです。

            // 5. 壁牌が0になったかチェック（鳴きがなければ）
            if (_wall.Count == 0)
            {
                Debug.WriteLine("[TurnManager] Wall is empty after discard. Exhaustive Draw.");
                return TurnResult.CreateExhaustiveDraw();
            }

            // 6. 次のプレイヤーへ (鳴きもロンもなければ)
            _currentTurnSeat = (_currentTurnSeat + 1) % _players.Count;
            Debug.WriteLine($"[TurnManager] Advancing to next turn: Player {_currentTurnSeat}");
            return TurnResult.CreateContinue();
        }

        /// <summary>
        /// 鳴きが発生した後のターン処理 (鳴いたプレイヤーが打牌する)
        /// </summary>
        public async Task<TurnResult> ProcessTurnAfterMeldAsync(Player melder)
        {
            _currentTurnSeat = melder.SeatIndex; // ターンを鳴いたプレイヤーに設定
            Player currentPlayer = melder;
            Debug.WriteLine($"[TurnManager] ----- Turn After Meld: Player {_currentTurnSeat} ({currentPlayer.Name}) -----");

            // カンの場合は嶺上開花、ドラめくりなどがあるが、ここでは省略。
            // ポン・チー・大明槓の後は打牌。

            Tile? tileToDiscard;
            if (currentPlayer.IsHuman)
            {
                if (_requestHumanDiscardCallback == null) return TurnResult.CreateError();
                tileToDiscard = await _requestHumanDiscardCallback.Invoke();
                if (tileToDiscard == null) return TurnResult.CreateError(); // GameManagerがnullを返さない前提
            }
            else
            {
                await Task.Delay(Config.Instance.AiThinkTimeMs);
                tileToDiscard = currentPlayer.ChooseDiscardTile();
                if (tileToDiscard == null) tileToDiscard = currentPlayer.Hand.LastOrDefault(); // フォールバック
                if (tileToDiscard == null) return TurnResult.CreateError(); // 手牌がないのは異常
                currentPlayer.DiscardTile(tileToDiscard);
            }

            LastDiscardedTile = tileToDiscard;
            currentPlayer.IsTsumo = false;
            Debug.WriteLine($"[TurnManager] Player {_currentTurnSeat} (after meld) discarded: {LastDiscardedTile.Name()}");
            _refreshUIDisplayCallback?.Invoke();

            // 再度、他家のアクションチェック
            // TODO: CallManagerを使用
            // ...

            if (_wall.Count == 0) return TurnResult.CreateExhaustiveDraw();

            _currentTurnSeat = (_currentTurnSeat + 1) % _players.Count;
            return TurnResult.CreateContinue();
        }

        // 既存のメソッド (StartTurn, DiscardByAI, DiscardTile, NextTurn, EndRound, IsHumanTurn) は
        // 上記の ProcessCurrentTurnAsync に統合されたり、役割が変わったりするため、
        // 必要に応じてリファクタリングまたは削除します。
        // 例えば、DiscardTileはPlayerクラスの責務に移す方がより自然です。
        // ここでは、ProcessCurrentTurnAsync内で直接Playerオブジェクトのメソッドを呼んでいます。
    }
}