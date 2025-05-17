
namespace MahjongApp
{
    public enum Wind { East = 0, South = 1, West = 2, North = 3 };
    public enum Suit { Manzu, Pinzu, Souzu, Honor };
    public enum MeldType { Chi, Pon, Kan, Kakan, Ankan, Minkan, Koutsu, Shuntsu, KokushiMelds, ChiitoitsuMelds }

    public enum GamePhase
    {
        InitRound,
        DrawPhase,
        DiscardPhase,
        CallCheckPhase,
        TurnEndPhase,
        RoundOver,
        GameOver,
        WinOrDrawProcessing
    }


    public enum TurnActionResult
    {
        Continue,        // 通常通り次のプレイヤーのターンへ
        Win,             // 和了が発生した
        ExhaustiveDraw,  // 荒牌平局（壁牌ゼロ）
        AbortiveDraw,    // 特殊流局（九種九牌など、実装する場合）
        MeldAndContinue, // 鳴きが発生し、鳴いたプレイヤーが続けて打牌する
        Error
    }
}