using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Engine;

public class MoveSearchEffortTable
{

    private readonly int[] Internal = new int[4096]; // 64 x 64

    public int this[Square from, Square to]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Internal.AA((int)from * 64 + (int)to);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Internal.AA((int)from * 64 + (int)to) = value;
    }

}