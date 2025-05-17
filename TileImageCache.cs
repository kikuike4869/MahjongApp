using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms; // Required for Image

namespace MahjongApp
{
    /// <summary>
    /// 麻雀牌の画像をキャッシュし、効率的なアクセスを提供します。
    /// </summary>
    public static class TileImageCache
    {
        private static readonly Dictionary<string, Image> _imageCache = new Dictionary<string, Image>();
        private static readonly string _basePath = "Resources/Tiles"; // 画像フォルダへのパス

        /// <summary>
        /// 指定された牌に対応する画像を取得します。キャッシュにあればそれを返し、なければ読み込みます。
        /// </summary>
        /// <param name="tile">画像を取得する牌。</param>
        /// <returns>牌の画像。</returns>
        /// <exception cref="FileNotFoundException">画像ファイルが見つからない場合にスローされます。</exception>
        public static Image GetImage(Tile tile)
        {
            if (tile == null)
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] TileImageCache.GetImage called with null tile.");
                // nullの場合の代替画像や例外処理を検討
                // 例えば、デフォルトの画像や透明な画像を返すなど
                // return GetDefaultTileImage(); // 仮のメソッド
                throw new ArgumentNullException(nameof(tile), "Tile object cannot be null.");
            }
            string tileKey = tile.ToString(); // キャッシュキー（例: "Manzu_1", "Pinzu_5_red"）

            if (_imageCache.TryGetValue(tileKey, out Image? cachedImage))
            {
                return cachedImage;
            }
            else
            {
                string filePath = Path.Combine(Application.StartupPath, _basePath, $"{tileKey}.png");

                if (File.Exists(filePath))
                {
                    // ファイルから読み込み、メモリにコピーしてファイルロックを解除
                    using (var originalImage = Image.FromFile(filePath))
                    {
                        var newImage = new Bitmap(originalImage);
                        _imageCache[tileKey] = newImage; // キャッシュに追加
                        return newImage;
                    }
                }
                else
                {
                    //代替画像やエラー処理を追加することも可能
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Image file not found: {filePath}");
                    // return some_default_image; // 例: デフォルト画像を返す
                    throw new FileNotFoundException($"Image file not found: {filePath}");
                }
            }
        }

        /// <summary>
        /// キャッシュされているすべての画像を破棄します (アプリケーション終了時などに使用)。
        /// </summary>
        public static void DisposeCache()
        {
            foreach (var image in _imageCache.Values)
            {
                image?.Dispose();
            }
            _imageCache.Clear();
        }
    }
}