namespace MahjongApp
{
    class ScoreManager
    {
        private List<Player> players;         // プレイヤー4人
        private int honbaCount;               // 本場（連荘）
        private int riichiSticks;             // リーチ棒（場にある）

        public int LastPointsMoved { get; private set; } // 前回の移動点数（UI表示用など）

        public ScoreManager(List<Player> players) { }

        // public void ProcessTsumoWin(Player winner, Tile winningTile, bool isDealer, List<Yaku> yakuList);
        // public void ProcessRonWin(Player winner, Player discarder, Tile winningTile, bool isDealer, List<Yaku> yakuList);

        // public void ProcessDraw(List<Player> tenpaiPlayers);
        // public void AddRiichiStick(Player declarer);     // リーチ宣言時
        // public void TransferRiichiSticks(Player winner); // 和了時に取得

        // public void AddHonba();         // 本場を加算
        // public void ResetHonba();       // 本場をリセット
        // public int GetPlayerPoints(int seatIndex);
    }
}