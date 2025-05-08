namespace MahjongApp
{
    class TurnManager
    {
        private int currentTurnSeat;               // 現在の手番プレイヤー（0〜3）
        private int dealerSeat;                    // 親の席番号（0〜3）

        private List<Player> players;
        private Deck Deck;
        private CallManager callManager;
        private ScoreManager scoreManager;

        private int turnCount;                     // 累積手番数（流局判定用）

        // public TurnManager(List<Player> players, Deck deck, CallManager callMgr, ScoreManager scoreMgr, int dealerSeat);

        // public void StartNewRound();                    // 局の開始
        // public void StartTurn();                        // 1手番の開始（ツモ・処理）
        // public void ProceedAfterDiscard(Tile discardedTile); // 打牌後の鳴きチェックと次手番へ

        // public void ExecuteCall(Player caller, CallType callType, List<Tile> tilesUsed); // チー・ポン・カン処理
        // public void ExecuteWin(Player winner, Player loser, Tile winTile, bool isTsumo); // 和了処理

        // public bool CheckDraw();                        // 山切れ or 九種九牌等
        // public void EndRound();                         // 局の終了
    }
}