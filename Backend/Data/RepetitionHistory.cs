using System;
using System.Runtime.CompilerServices;

namespace Backend.Data;

public class RepetitionHistory
{

    private const int SIZE = 1024;
    
    private readonly ulong[] Internal = GC.AllocateUninitializedArray<ulong>(SIZE);
    private int Index;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ulong zobristHash) => Internal.AA(Index++) = zobristHash;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveLast() => Index--;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Count(ulong zobristHash)
    {
        int count = 0;
        for (int i = Index; i > -1; i--) if (Internal.AA(i) == zobristHash) count++;
        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RepetitionHistory Clone()
    {
        RepetitionHistory history = new();
        Array.Copy(Internal, history.Internal, Index + 1);
        history.Index = Index;
        return history;
    }

}