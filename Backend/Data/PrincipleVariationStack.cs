using System.Runtime.CompilerServices;
using Backend.Data.Struct;

namespace Backend.Data;

public class PrincipleVariationStack
{

    private const int SIZE = 64;

    private readonly SearchedMove[] Internal = new SearchedMove[SIZE];
    private int Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchedMove Head() => Internal.AA(0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(SearchedMove move) => Internal.AA(Count++) = move;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(SearchedMove move)
    {
        for (int i = 0; i < Count; i++) if (Internal.AA(i) == move) return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset() => Count = 0;

}