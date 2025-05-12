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
                await DiscardPhase();
                NextTurn();
            }
        }

        public async Task DiscardPhase()
        {
            CurrentPhase = GamePhase.DiscardPhase;
            Console.WriteLine($"Entered {GamePhase.DiscardPhase} phase."); // デバッグログ

            if (TurnManager.IsHumanTurn())
            {
                var discardTaskCompletion = new TaskCompletionSource<bool>();

                // 捨て牌が完了したら待機解除する
                SetHumanPlayerDiscardCallback(() =>
                {
                    discardTaskCompletion.TrySetResult(true); // 待機解除
                    Console.WriteLine("Discard phase completed for human."); // デバッグログ
                });

                // タスク完了を待つ
                await discardTaskCompletion.Task;
            }
            else
            {
                TurnManager.DiscardByAI();
                Console.WriteLine("Discard phase completed for AI."); // デバッグログ
            }
        }

        public void NextTurn()
        {
            TurnManager.NextTurn();
            RefreshHandDisplay?.Invoke();
            TurnManager.StartTurn();
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
        }

        private Action? onHumanDiscard = null;

        public void SetHumanPlayerDiscardCallback(Action callback)
        {
            onHumanDiscard = callback;
        }

        public void NotifyHumanDiscard()
        {
            Console.WriteLine("NotifyHumanDiscard called.");
            onHumanDiscard?.Invoke();
            onHumanDiscard = null;
        }

        public void Test()
        {
            StartGame();
        }
    }
}