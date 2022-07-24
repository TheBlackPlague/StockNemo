using System;
using System.Runtime.CompilerServices;

namespace Backend.Data;

public class LateMovePruningTable
{

    private const int SIZE = 2 * 7;

    private readonly int[] Internal = new int[SIZE];

    public LateMovePruningTable()
    {
        for (int depth = 1; depth < 7; depth++) {
            Internal[7 + depth] = (int)(3.17 + 3.66 * Math.Pow(depth, 1.09));
            Internal[0 + depth] = (int)(-1.25 + 3.13 * Math.Pow(depth, 0.65));
        }
    }

    public int this[int type, int depth]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Internal.AA(type * 7 + depth);
    }

}