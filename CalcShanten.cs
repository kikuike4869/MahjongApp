using System;
using System.Collections.Generic;

namespace MahjongApp;

public class ShantenCalculator
{
    static readonly byte[] manzu = new byte[9];
    static readonly byte[] pinzu = new byte[9];
    static readonly byte[] souzu = new byte[9];
    static readonly byte[] jihai = new byte[7];

    // public static int CalculateShanten(List<Tile> hand, List<Meld> melds)
    // {
    //     ConvertTilesToBytes(hand, melds);

    //     ulong manzuPacked = SyantenMap.ShupaiMap[Hasher.GetIndex(manzu)];
    //     ulong pinzuPacked = SyantenMap.ShupaiMap[Hasher.GetIndex(pinzu)];
    //     ulong souzuPacked = SyantenMap.ShupaiMap[Hasher.GetIndex(souzu)];
    //     ulong jihaiPacked = SyantenMap.JipaiMap[Hasher.GetIndex(jihai)];

    //     int minShanten = 8;
    //     int meldCount = melds.Count;

    //     for (int m = 0; m < 9; m++)
    //     {
    //         int mMentsu = m % 5, mJantou = m / 5;
    //         int mValue = GetUnpackedValue(manzuPacked, m);

    //         for (int p = 0; p < 9; p++)
    //         {
    //             int pMentsu = p % 5, pJantou = p / 5;
    //             int pValue = GetUnpackedValue(pinzuPacked, p);

    //             for (int s = 0; s < 9; s++)
    //             {
    //                 int sMentsu = s % 5, sJantou = s / 5;
    //                 int sValue = GetUnpackedValue(souzuPacked, s);

    //                 for (int j = 0; j < 9; j++)
    //                 {
    //                     int jMentsu = j % 5, jJantou = j / 5;
    //                     int jValue = GetUnpackedValue(jihaiPacked, j);

    //                     int totalMentsu = meldCount + mMentsu + pMentsu + sMentsu + jMentsu;
    //                     int totalJantou = mJantou + pJantou + sJantou + jJantou;

    //                     if (totalMentsu > 4 || totalJantou > 1) continue;

    //                     int currentShanten = 8 - (2 * totalMentsu) - totalJantou +
    //                                          mValue + pValue + sValue + jValue;

    //                     minShanten = Math.Min(minShanten, currentShanten);
    //                 }
    //             }
    //         }
    //     }

    //     return minShanten;
    // }

    public static int CalculateShanten(List<Tile> hand, List<Meld> melds)
    {
        ConvertTilesToBytes(hand, melds);

        ulong manzuPacked = SyantenMap.ShupaiMap[Hasher.GetIndex(manzu)];
        ulong pinzuPacked = SyantenMap.ShupaiMap[Hasher.GetIndex(pinzu)];
        ulong souzuPacked = SyantenMap.ShupaiMap[Hasher.GetIndex(souzu)];
        ulong jihaiPacked = SyantenMap.JipaiMap[Hasher.GetIndex(jihai)];

        int minShanten = 8;
        int meldCount = melds.Count;

        for (int m = 0; m < 9; m++)
        {
            int mMentsu = m % 5, mJantou = m / 5;
            int mValue = GetUnpackedValue(manzuPacked, m);

            for (int p = 0; p < 9; p++)
            {
                int pMentsu = p % 5, pJantou = p / 5;
                int pValue = GetUnpackedValue(pinzuPacked, p);

                for (int s = 0; s < 9; s++)
                {
                    int sMentsu = s % 5, sJantou = s / 5;
                    int sValue = GetUnpackedValue(souzuPacked, s);

                    for (int j = 0; j < 9; j++)
                    {
                        int jMentsu = j % 5, jJantou = j / 5;
                        int jValue = GetUnpackedValue(jihaiPacked, j);

                        int totalMentsu = meldCount + mMentsu + pMentsu + sMentsu + jMentsu;
                        int totalJantou = mJantou + pJantou + sJantou + jJantou;

                        if (totalMentsu != 4 || totalJantou != 1) continue;

                        int currentShanten = mValue + pValue + sValue + jValue;

                        minShanten = Math.Min(minShanten, currentShanten);
                    }
                }
            }
        }

        return minShanten;
    }

    static void ConvertTilesToBytes(List<Tile> hand, List<Meld> melds)
    {
        Array.Clear(manzu, 0, manzu.Length);
        Array.Clear(pinzu, 0, pinzu.Length);
        Array.Clear(souzu, 0, souzu.Length);
        Array.Clear(jihai, 0, jihai.Length);

        ProcessTiles(hand);

        foreach (Meld meld in melds)
        {
            if (meld.Type == MeldType.Chi)
            {
                ProcessTiles(meld.Tiles);
            }
            else
            {
                IncrementTileCount(meld.Tiles[0], 3);
            }
        }
    }

    static void ProcessTiles(IEnumerable<Tile> tiles)
    {
        foreach (Tile tile in tiles)
        {
            IncrementTileCount(tile, 1);
        }
    }

    static void IncrementTileCount(Tile tile, int count)
    {
        int index = tile.Number - 1;
        switch (tile.Suit)
        {
            case Suit.Manzu:
                manzu[index] += (byte)count;
                break;
            case Suit.Pinzu:
                pinzu[index] += (byte)count;
                break;
            case Suit.Souzu:
                souzu[index] += (byte)count;
                break;
            case Suit.Honor:
                jihai[index] += (byte)count;
                break;
        }
    }

    static int GetUnpackedValue(ulong packedValue, int index)
    {
        int shift = 3 * (5 * (index / 5) + (index % 5)) - 3;
        return (int)((packedValue >> shift) & 0x7);
    }
}
