namespace MahjongApp
{
    class Deck
    {
        private List<Tile> allTiles;            // シャッフル済み136枚の山
        private Queue<Tile> liveWall;           // 通常のツモ山（最初の122枚）
        private Queue<Tile> deadWall;           // 嶺上牌＋ドラ表示牌（14枚）

        public List<Tile> DoraIndicators { get; private set; } = new(); // 表ドラ
        public List<Tile> UraDoraIndicators { get; private set; } = new(); // 裏ドラ（リーチ時用）

        // public void InitializeWall(bool useRedDora);   // 山の生成＋シャッフル
        // public List<Tile> DrawInitialHand();           // 配牌用（13枚）
        // public Tile DrawTile();                        // 通常のツモ
        // public Tile DrawFromDeadWall();                // 嶺上牌からツモ
        // public Tile RevealNextDora();                  // カンでドラ追加
        // public Tile RevealUraDora();                   // 裏ドラ表示（リーチのみ）
        // public int RemainingTiles();                   // ツモ山残り枚数（流局判定用）}
    }
}