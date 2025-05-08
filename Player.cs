namespace MahjongApp
{
    class Player
    {
        public string Name { get; set; } = "unknown";
        public int SeatIndex { get; set; } // 0=自分, 1=下家, 2=対面, 3=上家
        public int Points { get; set; } = 25000;

        public List<Tile> Hand { get; private set; } = new();
        public List<Tile> Discards { get; private set; } = new();
        public List<Meld> Melds { get; private set; } = new();

        public bool IsDealer { get; set; } = false;
        public bool HasDeclaredRiichi { get; private set; } = false;
        public bool IsIppatsu { get; set; } = false;
        public bool IsTenpai { get; set; } = false;

        public bool IsHuman { get; protected set; } = false;

        // public void Draw(Tile tile);                  // ツモ
        // public virtual Tile ChooseDiscard();          // 捨てる牌を選ぶ
        // public void Discard(Tile tile);               // 捨てる処理
        // public void DeclareRiichi();                  // リーチ宣言
        // public void AddMeld(Meld meld);               // ポン・チー・カンなど
        // public bool CheckWin(Tile drawnOrClaimedTile);// 和了可能かチェック
        // public void SortHand();                       // 手牌をソート（見やすさ用）
    }

    class HumanPlayer : Player
    {
        public HumanPlayer() { IsHuman = true; }

        // public override Tile ChooseDiscard()
        // {
        //     // UI側のクリック入力などで選択された牌を返す
        //     return AwaitUserTileSelection();
        // }
    }

    class AIPlayer : Player
    {
        public AIPlayer() { IsHuman = false; }

        // public override Tile ChooseDiscard()
        // {
        //     // 初期段階では完全ランダム
        //     return base.ChooseDiscard();
        //     // 将来的には牌効率アルゴリズムに置き換え可
        // }
    }

    enum MeldType { Chi, Pon, Kan, Ankan, Shouminkan }

    class Meld
    {
        public MeldType Type { get; set; }
        public List<Tile> Tiles { get; set; } = new List<Tile>();
        public int FromPlayerIndex { get; set; } // 鳴いた相手
    }
}