using System.Net.Security;

namespace MahjongApp
{
    public enum Suit
    {
        Manzu,  // 萬子
        Pinzu,  // 筒子
        Souzu,  // 索子
        Honor   // 字牌（東南西北白發中）
    }

    public class Tile
    {
        public Suit Suit { get; private set; }
        public int Rank { get; private set; }         // 1-9 for 数牌, 1-7 for 字牌 (東=1, 南=2, ..., 中=7)
        public bool IsRed { get; private set; }

        public Tile(Suit suit, int rank, bool isRed = false)
        {
            Suit = suit;
            Rank = rank;
            IsRed = isRed;
        }
        // public override string ToString();
    }
}
