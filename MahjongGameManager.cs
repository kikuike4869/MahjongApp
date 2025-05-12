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


        int RoundNumber;
        int HonbaCount;
        int RiichiSticks;
        int DealerIndex;

        public MahjongGameManager()
        {
            Deck = new Deck();
            Players = new List<Player>();

            InitializGame();
            CallManager = new CallManager(Players, DealerIndex);
            ScoreManager = new ScoreManager();

            TurnManager = new TurnManager(Players, Deck, CallManager, ScoreManager, DealerIndex);
        }

        void InitializGame()
        {
            DealerIndex = 0;

            for (int i = 0; i < Config.Instance.NumberOfPlayers; i++)
            {
                if (i == 0)
                    Players.Add(new HumanPlayer());
                else
                    Players.Add(new AIPlayer());
            }
        }

        public async Task StartGame()
        {
            TurnManager.StartNewRound();
            RefreshHandDisplay?.Invoke();

            while (Deck.Count > 0)
            {
                try
                {
                    Console.WriteLine($"[DEBUG] MahjongGameManager: Starting await DiscardPhase() for turn seat: {TurnManager.GetCurrentTurnSeat()}"); // TurnManagerに現在のSeatを取得するメソッドを追加する必要があるかもしれません
                    await DiscardPhase();
                    Console.WriteLine("[DEBUG] MahjongGameManager: DiscardPhase completed. Calling NextTurn()");
                    NextTurn();
                    Console.WriteLine("[DEBUG] MahjongGameManager: NextTurn completed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FATAL ERROR] Exception in StartGame loop: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    // ここでループを中断するか、エラー処理を行う
                    break;
                }
            }
            Console.WriteLine("[DEBUG] MahjongGameManager: Game loop finished (Deck empty?).");
        }

        public async Task DiscardPhase()
        {
            CurrentPhase = GamePhase.DiscardPhase;
            // Console.WriteLine($"Entered {GamePhase.DiscardPhase} phase."); // デバッグログ

            if (TurnManager.IsHumanTurn())
            {
                var discardTaskCompletion = new TaskCompletionSource<bool>();

                // 捨て牌が完了したら待機解除する
                // TurnManager.SetHumanPlayerDiscardCallback(() =>
                // {
                //     discardTaskCompletion.TrySetResult(true); // 待機解除
                //     Console.WriteLine("Discard phase completed for human."); // デバッグログ
                // });
                TurnManager.SetHumanPlayerDiscardCallback(() =>
                {
                    Console.WriteLine("[DEBUG] Callback: Attempting TrySetResult."); // 追加
                    bool setResult = discardTaskCompletion.TrySetResult(true); // 結果を変数に受ける
                    Console.WriteLine($"[DEBUG] Callback: TrySetResult successful: {setResult}."); // 結果をログ出力 (追加)
                    Console.WriteLine("Discard phase completed for human.");
                });

                // タスク完了を待つ
                await discardTaskCompletion.Task.ConfigureAwait(false);
                Console.WriteLine("[DEBUG] MahjongGameManager: DiscardPhase await completed for human.");
            }
            else
            {
                TurnManager.DiscardByAI();
                // Console.WriteLine("Discard phase completed for AI."); // デバッグログ
            }
        }

        public void NextTurn()
        {
            Console.WriteLine("[DEBUG] MahjongGameManager: NextTurn() called.");
            Console.WriteLine("[DEBUG] MahjongGameManager: Calling TurnManager.NextTurn()");
            TurnManager.NextTurn();
            Console.WriteLine("[DEBUG] MahjongGameManager: TurnManager.NextTurn() returned.");
            RefreshHandDisplay?.Invoke();
            Console.WriteLine("[DEBUG] MahjongGameManager: Calling TurnManager.StartTurn()");
            TurnManager.StartTurn();
            Console.WriteLine("[DEBUG] MahjongGameManager: TurnManager.StartTurn() returned.");
        }

        public void FinishTurn()
        {
            Console.WriteLine("Called FinishTurn.");
            CurrentPhase = GamePhase.MakeDecision;
        }

        public HumanPlayer GetHumanPlayer()
        {
            foreach (Player player in Players)
            {
                if (player.IsHuman)
                {
                    return (HumanPlayer)player;
                }
            }

            return null;
        }


        // void StartRound();
        // void ProcessTurn();
        // bool CheckWinOrDraw();
        // void EndRound();
        // void AdvanceRound();
        // void EndGame();

        Action RefreshHandDisplay;
        public void SetUpdateUICallBack(Action refreshHandDisplay)
        {
            RefreshHandDisplay = refreshHandDisplay;
            TurnManager.SetUpdateUICallBack(refreshHandDisplay);
        }
        public void NotifyHumanDiscardOfTurnManager()
        {
            Console.WriteLine("[DEBUG] MahjongGameManager: NotifyHumanDiscardOfTurnManager called. Forwarding to TurnManager.");
            TurnManager.NotifyHumanDiscard();
        }

        public void Test()
        {
            StartGame();
        }
    }
}