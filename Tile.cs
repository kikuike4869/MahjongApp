using System.Drawing;
using System.IO; // Keep for potential future use, but GetImage logic moved

namespace MahjongApp
{
    public class Tile
    {
        public Suit Suit { get; private set; }
        public int Index { get; private set; }
        public int Number { get; private set; }         // 1-9 for 数牌, 1-7 for 字牌 (東=1, 南=2, ..., 中=7)
        public bool IsRed { get; private set; }
        public bool IsSelected { get; set; }

        public Tile(Suit suit, int number, int index, bool isRed = false, bool isSelected = false)
        {
            Suit = suit;
            Number = number;
            Index = index;
            IsRed = isRed;
            IsSelected = isSelected;
        }

        /// <summary>
        /// 画像ファイル名やキャッシュキーとして使用される文字列表現を取得します。
        /// 例: "Manzu_5", "Souzu_5_red", "Honor_1"
        /// </summary>
        public override string ToString()
        {
            // 赤ドラの場合 "_red" を付加
            return $"{Suit}_{Number}{(IsRed ? "_red" : "")}";
        }

        /// <summary>
        /// デバッグや識別に使うための詳細な名前を取得します (Indexを含む)。
        /// 例: "Manzu_5_0", "Souzu_5_3"
        /// </summary>
        public string Name()
        {
            return $"{Suit}_{Number}_{Index}";
        }

        /// <summary>
        /// この牌に対応する画像を取得します (キャッシュを利用)。
        /// </summary>
        /// <returns>牌の画像。</returns>
        public Image GetImage()
        {
            // TileImageCache クラス経由で画像を取得
            return TileImageCache.GetImage(this);
        }

        public bool EqualsForDora(Tile other)
        {
            if (other == null) return false;
            // 赤ドラは通常の数牌としてもドラとしてカウントされるため、SuitとNumberのみで比較
            return this.Suit == other.Suit && this.Number == other.Number;
        }
    }
}
