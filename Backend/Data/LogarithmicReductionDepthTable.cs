using System;
using System.Runtime.CompilerServices;

namespace Backend.Data;

public class LogarithmicReductionDepthTable
{

    private const int SIZE = 64 * 64;

    private readonly int[] Internal = new int[SIZE];

    public LogarithmicReductionDepthTable()
    {
        for (int depth = 1; depth < 64; depth++)
        for (int played = 1; played < 64; played++) {
            Internal[depth * 64 + played] = Math.Max((int)Math.Log(depth) * (int)Math.Log(played), 1);
        }
    }

    public int this[int depth, int played]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Internal.AA(depth * 64 + played);
    }

}