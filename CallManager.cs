namespace MahjongApp
{
    public class CallManager
    {
        private List<Player> players;
        private int dealerSeat;

        public CallManager(List<Player> players, int dealerSeat)
        {
            this.players = players;
            this.dealerSeat = dealerSeat;
        }

        // /// 捨て牌に対する鳴き・ロンの応答を確認（各プレイヤーに問い合わせ）
        // public List<CallResponse> CheckCalls(Tile discardedTile, int discarderSeat);
    }
}