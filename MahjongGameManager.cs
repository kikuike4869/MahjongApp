namespace MahjongApp
{
    public class MahjongGameManager
    {
        List<Player> Players;
        Deck Deck;
        TurnManager TurnManager;
        // CallManager CallManager;
        ScoreManager ScoreManager;

        int RoundNumber;
        int HonbaCount;
        int RiichiSticks;
        int DealerIndex;

        public MahjongGameManager()
        {
            Deck = new Deck();
            Players = new List<Player>();
            TurnManager = new TurnManager();
            // CallManager = new CallManager();
            ScoreManager = new ScoreManager();

            InitializGame();
            StartGame();
        }

        void InitializGame()
        {
            for (int i = 0; i < Config.Instance.NumberOfPlayers - 1; i++)
            {
                if (i == 0)
                    Players.Add(new HumanPlayer());
                else
                    Players.Add(new AIPlayer());
            }

            foreach (Player player in Players)
            {
                for (int i = 0; i < Config.Instance.NumberOfFirstHands; i++)
                    player.Draw(Deck.Draw());
            }
        }

        public List<Tile> GetHumanPlayerHand()
        {
            foreach (Player player in Players)
            {
                if (player.IsHuman)
                {
                    return player.Hand;
                }
            }
            return new List<Tile>();
        }

        void StartGame()
        {
        }
        // void StartRound();
        // void ProcessTurn();
        // bool CheckWinOrDraw();
        // void EndRound();
        // void AdvanceRound();
        // void EndGame();
    }
}