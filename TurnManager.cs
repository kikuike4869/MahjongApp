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
            Console.WriteLine($"[DEBUG] TurnManager: StartTurn() called for CurrentTurnSeat: {CurrentTurnSeat}");
            Console.WriteLine($"Deck Count: {Deck.Count}");
            if (Deck.Count == 0)
            {
                Console.WriteLine("[DEBUG] TurnManager: Deck is empty, calling EndRound.");
                EndRound();
                return;
            }

            Player currentPlayer = Players[CurrentTurnSeat];
            currentPlayer.IsTsumo = true;
            Tile drawn = Deck.Draw();
            currentPlayer.Draw(drawn);
            RefreshHandDisplay?.Invoke();

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
            Console.WriteLine($"[DEBUG] TurnManager: NextTurn() called. CurrentTurnSeat BEFORE: {CurrentTurnSeat}");
            Players[CurrentTurnSeat].IsTsumo = false;
            CurrentTurnSeat = (CurrentTurnSeat + 1) % Config.Instance.NumberOfPlayers;
            Console.WriteLine($"[DEBUG] TurnManager: NextTurn() finished. CurrentTurnSeat AFTER: {CurrentTurnSeat}");
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

        Action? RefreshHandDisplay = null;
        public void SetUpdateUICallBack(Action refreshHandDisplay)
        {
            RefreshHandDisplay = refreshHandDisplay;
        }

        private Action? OnHumanDiscard = null;
        public void SetHumanPlayerDiscardCallback(Action onHumanDiscard)
        {
            OnHumanDiscard = onHumanDiscard;
        }

        public void NotifyHumanDiscard()
        {
            // Console.WriteLine("NotifyHumanDiscard called.");
            Console.WriteLine("[DEBUG] TurnManager: NotifyHumanDiscard called. Invoking callback.");
            OnHumanDiscard?.Invoke();
            OnHumanDiscard = null;
        }


        public int GetCurrentTurnSeat()
        {
            return CurrentTurnSeat;
        }
    }
}