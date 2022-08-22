using System.Runtime.CompilerServices;
using Backend.Data.Struct;

namespace Backend.Data;

public class KillerMoveTable
{

    private const int SIZE = 128;

    private readonly OrderedMoveEntry[] Internal = new OrderedMoveEntry[2 * SIZE];

    public OrderedMoveEntry this[int type, int ply]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Internal.AA(type * SIZE + ply);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Internal.AA(type * SIZE + ply) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReOrder(int ply) => Internal.AA(SIZE + ply) = Internal.AA(ply);

}