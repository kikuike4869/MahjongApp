// TurnResult.cs
namespace MahjongApp
{
    public class TurnResult
    {
        public TurnActionResult ResultType { get; private set; }
        public Player? WinningPlayer { get; private set; } // 和了した場合
        public Tile? WinningTile { get; private set; }     // 和了牌
        public Player? MeldPlayer { get; private set; }    // 鳴いたプレイヤー
        public Meld? MeldAction { get; private set; }      // 鳴きの種類と牌
        public bool IsSelfWin { get; set; } // ツモ和了かロン和了か

        private TurnResult(TurnActionResult resultType)
        {
            ResultType = resultType;
        }

        public static TurnResult CreateContinue()
        {
            return new TurnResult(TurnActionResult.Continue);
        }

        public static TurnResult CreateWin(Player winner, Tile winningTile, bool isSelfWin)
        {
            return new TurnResult(TurnActionResult.Win)
            {
                WinningPlayer = winner,
                WinningTile = winningTile,
                IsSelfWin = isSelfWin
            };
        }

        public static TurnResult CreateExhaustiveDraw()
        {
            return new TurnResult(TurnActionResult.ExhaustiveDraw);
        }

        public static TurnResult CreateMeldAndContinue(Player melder, Meld meldAction)
        {
            return new TurnResult(TurnActionResult.MeldAndContinue)
            {
                MeldPlayer = melder,
                MeldAction = meldAction
            };
        }

        public static TurnResult CreateError()
        {
            return new TurnResult(TurnActionResult.Error);
        }
    }
}