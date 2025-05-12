namespace MahjongApp
{
    class TurnManager
    {
        private int CurrentTurnSeat;               // 現在の手番プレイヤー（0〜3）
        private int DealerSeat;                    // 親の席番号（0〜3）

        private List<Player> Players;
        private Deck Deck;
        private CallManager SallMgr;
        private ScoreManager ScoreMgr;

        private int TurnCount;                     // 累積手番数（流局判定用）

        public TurnManager(List<Player> players, Deck deck, CallManager callMgr, ScoreManager scoreMgr, int dealerSeat)
        {
            this.Players = players;
            this.Deck = deck;
            this.SallMgr = callMgr;
            this.ScoreMgr = scoreMgr;
            this.DealerSeat = dealerSeat;
        }

        public void StartNewRound()
        {
            Console.WriteLine("Start New Round");

            TurnCount = 0;
            CurrentTurnSeat = DealerSeat;


            foreach (Player player in Players)
            {
                for (int i = 0; i < Config.Instance.NumberOfFirstHands; i++)
                    player.Draw(Deck.Draw());
            }

            foreach (Player player in Players)
            {
                player.SortHand();
            }

            StartTurn();
        }

        public void StartTurn()
        {
            Console.WriteLine($"Deck Count: {Deck.Count}");
            if (Deck.Count == 0)
            {
                EndRound();
            }

            Player currentPlayer = Players[CurrentTurnSeat];
            currentPlayer.IsTsumo = true;
            Tile drawn = Deck.Draw();
            currentPlayer.Draw(drawn);

            // if (currentPlayer.CanTsumo())
            // {
            //     ExecuteWin(currentPlayer, null, drawn, true);
            //     return;
            // }

            // if (currentPlayer.CanDeclareKan())
            // {
            //     // プレイヤーがカン宣言するかどうかの判断が必要
            // }

        }

        public void NextTurn()
        {
            Players[CurrentTurnSeat].IsTsumo = false;
            CurrentTurnSeat = (CurrentTurnSeat + 1) % Config.Instance.NumberOfPlayers;
            // StartTurn();
        }
        // public void ProceedAfterDiscard(Tile discardedTile); // 打牌後の鳴きチェックと次手番へ

        // public void ExecuteCall(Player caller, CallType callType, List<Tile> tilesUsed); // チー・ポン・カン処理
        // public void ExecuteWin(Player winner, Player loser, Tile winTile, bool isTsumo); // 和了処理

        // public bool CheckDraw();                        // 山切れ or 九種九牌等

        public void EndRound()
        {
            Console.WriteLine("End Round");
        }
        public bool IsHumanTurn()
        {
            Console.WriteLine($"CurrentTurnSeat: {CurrentTurnSeat}, IsHuman: {Players[CurrentTurnSeat].IsHuman}");
            return Players[CurrentTurnSeat].IsHuman;
        }

        public void DiscardByAI()
        {
            Players[CurrentTurnSeat].DiscardTile();
        }

    }
}