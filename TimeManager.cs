namespace MahjongApp
{
    public class TimeManager
    {
        private const int ShortTimeLimit = 5;  // 秒（1手ごとの時間）
        private const int LongTimeMax = 20;    // 秒（1局ごとの時間）
        private int[] remainingLongTime;       // 各プレイヤーの長時間残量
        private CancellationTokenSource timerCts;

        public event Action<int> OnTimeout;    // タイムアウト時のコールバック（seat番号）

        // public TimeManager()
        // {
        //     remainingLongTime = new int[4]; // 4人分
        //     for (int i = 0; i < 4; i++) remainingLongTime[i] = LongTimeMax;
        // }

        // public void ResetLongTimeForRound()
        // {
        //     for (int i = 0; i < 4; i++) remainingLongTime[i] = LongTimeMax;
        // }

        // public int GetRemainingLongTime(int seat) => remainingLongTime[seat];
    }
}