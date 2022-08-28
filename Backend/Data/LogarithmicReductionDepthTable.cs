using System;
using System.Runtime.CompilerServices;

namespace Backend.Data;

public class LogarithmicReductionDepthTable
{

    private const int SIZE = 128;

    private readonly int[] Internal = new int[SIZE * SIZE];

    public LogarithmicReductionDepthTable()
    {
        for (int depth = 1; depth < SIZE; depth++)
        for (int played = 1; played < SIZE; played++) {
            Internal[depth * SIZE + played] = (int)(Math.Log(depth) * Math.Log(played) / 2 + 1);
        }
    }

    public int this[int depth, int played]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Internal.AA(depth * SIZE + played);
    }

}