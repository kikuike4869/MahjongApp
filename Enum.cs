
namespace MahjongApp
{
    public enum Wind { East = 0, South = 1, West = 2, North = 3 };
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