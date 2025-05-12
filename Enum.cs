
namespace MahjongApp
{
    public enum Wind { East, South, West, North };
    public enum Suit { Manzu, Pinzu, Souzu, Honor };
    public enum MeldType { Chi, Pon, Kan, ConcealedKan };
    public enum GamePhase
    {
        InitRound,       // ラウンド開始前の準備フェーズ（牌配布、風の決定など）
        PlayerDrawTile,  // プレイヤーがツモするフェーズ
        MakeDecision,    // 行動を決定する（捨てるか、鳴くか）
        DiscardPhase,    // プレイヤーが捨て牌するフェーズ
        ResolveActions,  // 副露（鳴く）や和了の解決
        Call,            // 誰かが鳴くフェーズ（ポン、チー、カンなど）
        CheckWinningHand,// 和了の役を確認するフェーズ（上がり判定）
        CalculateScore,  // 点数計算フェーズ
        OtherPlayerTurn, // 他プレイヤーまたはAIのターン
        EndRound,        // ラウンドの終了
        EndGame          // 全ゲーム終了
    }
}