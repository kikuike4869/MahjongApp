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

            for (int i = 0; i < Config.Instance.NumberOfPlayers - 1; i++)
            {
                if (i == 0)
                    Players.Add(new HumanPlayer());
                else
                    Players.Add(new AIPlayer());
            }
        }

        public void StartGame()
        {

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

        public void Test()
        {
            RefreshHandDisplay?.Invoke();
            StartGame();
        }
    }
}