namespace MahjongApp;

public static class Hasher
{
    static readonly int[,] cumulativeDp = new int[,]
   {
        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 2, 3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},
        {1, 3, 6, 10, 15, 19, 22, 24, 25, 25, 25, 25, 25, 25, 25},
        {1, 4, 10, 20, 35, 53, 72, 90, 105, 115, 121, 124, 125, 125, 125},
        {1, 5, 15, 35, 70, 122, 190, 270, 355, 435, 503, 555, 590, 610, 620},
        {1, 6, 21, 56, 126, 247, 432, 687, 1007, 1372, 1753, 2118, 2438, 2693, 2878},
        {1, 7, 28, 84, 210, 456, 882, 1548, 2499, 3745, 5251, 6937, 8688, 10374, 11880},
        {1, 8, 36, 120, 330, 785, 1660, 3180, 5595, 9130, 13925, 19980, 27120, 34995, 43130},
        {1, 9, 45, 165, 495, 1279, 2931, 6075, 11550, 20350, 33490, 51810, 75750, 105150, 139150},
        {1, 10, 55, 220, 715, 1993, 4915, 10945, 22330, 42185, 74396, 123275, 192950, 286550, 405350}
   };

    static byte[] ExtendToNine(byte[] w)
    {

        byte[] extendedHand = new byte[9];
        // はじめの2要素は0で埋める
        extendedHand[0] = 0;
        extendedHand[1] = 0;
        for (int i = 0; i < 7; i++)
        {
            extendedHand[i + 2] = w[i];
        }
        return extendedHand;
    }

    public static int GetIndex(byte[] w)
    {
        // 与えられた麻雀の手牌 w の最小完全ハッシュインデックスを計算します。

        // Args:
        //     w (byte[]): 9つのバイト値からなる配列。各バイトは0から4の間。
        //                 w の要素の合計は0から14の間でなければなりません。

        // Returns:
        //     int: 計算されたハッシュインデックス。

        // Raises:
        //     ArgumentException: w の形式が無効な場合。

        if (w == null || (w.Length != 9 && w.Length != 7))
        {
            throw new ArgumentException("手牌 w は9つもしくは7つの要素を持つ配列でなければなりません。", nameof(w));
        }
        if (!w.All(x => x >= 0 && x <= 4))
        {
            throw new ArgumentException("手牌 w の各要素は0から4のバイト値でなければなりません。", nameof(w));
        }
        if (w.Length == 7)
        {
            w = ExtendToNine(w);
        }

        int currentHandSum = w.Sum(x => (int)x);

        // Console.WriteLine($"Hand Sum: {currentHandSum}");

        if (currentHandSum > 14)
        {
            throw new ArgumentException($"手牌 w の要素の合計は0から14の間でなければなりません。現在の手牌の合計: {currentHandSum}", nameof(w));
        }

        int hIndex = 0;
        int currentPrefixSum = 0;  // 現在処理中のプレフィックスの合計値

        // Console.WriteLine($"hIndex: {hIndex}");
        // Console.WriteLine($"currentPrefixSum: {currentPrefixSum}");

        for (int i = 0; i < 9; i++)  // 手牌 w の現在の桁のインデックス (c_i)
        {
            // この桁より後ろに来るサフィックス（接尾辞）の要素数
            int numElementsInSuffix = 9 - (i + 1);

            // 現在の桁 c_i に c_i より小さい値 v を置いた場合を考える
            // v は 0 から w[i]-1 まで
            for (int valCurrentDigit = 0; valCurrentDigit < w[i]; valCurrentDigit++)
            {
                // (c_0 ... c_{i-1}) の合計 + v
                int sumOfPrefixWithV = currentPrefixSum + valCurrentDigit;

                // サフィックスが取りうる最大の合計値
                // (全体の合計が14を超えないようにするため)
                int maxSumForSuffix = 14 - sumOfPrefixWithV;

                int countForThisPrefixCombination = 0;
                if (maxSumForSuffix >= 0)
                {
                    // max_sum_for_suffix が負の場合、有効なサフィックスは存在しない (0通り)
                    // cumulativeDp[サフィックス長, サフィックスの最大許容合計]
                    countForThisPrefixCombination = cumulativeDp[numElementsInSuffix, maxSumForSuffix];
                }

                hIndex += countForThisPrefixCombination;
            }

            // 実際の現在の桁の値 w[i] をプレフィックスの合計に加える
            currentPrefixSum += w[i];
        }

        return hIndex;
    }
}
