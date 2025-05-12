
namespace MahjongApp
{
    public enum Wind { East, South, West, North };
    public enum Suit { Manzu, Pinzu, Souzu, Honor };
    public enum MeldType { Chi, Pon, Kan, Kakan, Ankan, Minkan }

    public enum GamePhase
    {
        InitRound,
        DrawPhase,
        DiscardPhase,
        CallCheckPhase,
        TurnEndPhase,
        RoundOver, // Includes win or draw calculation
        GameOver
    }
}