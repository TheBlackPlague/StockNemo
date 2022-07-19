using System.Runtime.CompilerServices;
using Backend.Data.Struct;

namespace Backend.Data;

public class KillerMoveTable
{

    private const int SIZE = 2 * 64;

    private readonly OrderedMoveEntry[] Internal = new OrderedMoveEntry[SIZE];

    public OrderedMoveEntry this[int type, int ply]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Internal.AA(type * 64 + ply);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Internal.AA(type * 64 + ply) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReOrder(int ply) => Internal.AA(64 + ply) = Internal.AA(ply);

}