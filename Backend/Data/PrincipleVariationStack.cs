using System;
using System.Runtime.CompilerServices;
using Backend.Data.Struct;

namespace Backend.Data;

public class PrincipleVariationStack
{

    private const int SIZE = 1024;
    
    public int Count { get; private set; }

    private readonly SearchedMove[] Internal = GC.AllocateUninitializedArray<SearchedMove>(SIZE);

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