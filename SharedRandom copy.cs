using System;

namespace MahjongApp
{
    /// <summary>
    /// アプリケーション全体で共有される Random インスタンスを提供します。
    /// new Random() を短時間で繰り返し呼び出すことによるシード値の重複を防ぎます。
    /// </summary>
    public static class SharedRandom
    {
        private static readonly Random _instance = new Random();

        /// <summary>
        /// 共有の Random インスタンスを取得します。
        /// </summary>
        public static Random Instance => _instance;
    }
}


