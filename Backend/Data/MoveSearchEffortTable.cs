using System;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data;

public class MoveSearchEffortTable
{

    private readonly int[] Internal = GC.AllocateUninitializedArray<int>(4096); // 64 x 64

    public int this[Square from, Square to]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Internal.AA((int)from * 64 + (int)to);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Internal.AA((int)from * 64 + (int)to) = value;
    }

}