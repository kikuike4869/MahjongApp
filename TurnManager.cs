namespace MahjongApp
{
    class TurnManager
    {
        private int currentTurnSeat;               // 現在の手番プレイヤー（0〜3）
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
            TurnCount = 0;
            currentTurnSeat = DealerSeat;

            
            foreach (Player player in Players)
            {
                for (int i = 0; i < Config.Instance.NumberOfFirstHands; i++)
                    player.Draw(Deck.Draw());
            }

            foreach (Player player in Players)
            {
                player.SortHand();
            }

        }
        // public void StartTurn();                        // 1手番の開始（ツモ・処理）
        // public void ProceedAfterDiscard(Tile discardedTile); // 打牌後の鳴きチェックと次手番へ

        // public void ExecuteCall(Player caller, CallType callType, List<Tile> tilesUsed); // チー・ポン・カン処理
        // public void ExecuteWin(Player winner, Player loser, Tile winTile, bool isTsumo); // 和了処理

        // public bool CheckDraw();                        // 山切れ or 九種九牌等
        // public void EndRound();                         // 局の終了
    }
}