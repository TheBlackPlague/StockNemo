using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Data;

public class PerftTranspositionTable
{

    private const int HASH_FILTER = 0xFFFFFFF;

    private readonly PerftTranspositionTableEntry[] Internal = 
        new PerftTranspositionTableEntry[HASH_FILTER + 1];

    public ulong HitCount;

    public PerftTranspositionTable()
    {
        int partitionLength = Internal.Length / (Environment.ProcessorCount * 2);
        Parallel.For(0, Environment.ProcessorCount * 2, p =>
        {
            int start = p * partitionLength;
            int end = start + partitionLength;
            for (int i = start; i < end; i++) Internal[i] = new PerftTranspositionTableEntry();
        });

        HitCount = 0;
    }

    public ulong this[ulong hash, int depth]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Interlocked.Increment(ref HitCount);
            return Internal[(int)hash & HASH_FILTER][depth];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            PerftTranspositionTableEntry entry = Internal[(int)hash & HASH_FILTER];
            if (!entry.Set) entry.SetZobristHash(hash);
            if (entry.ZobristHash != hash) return;
            entry[depth] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool VerifyDepth(ulong hash, int depth)
    {
        PerftTranspositionTableEntry entry = Internal[(int)hash & HASH_FILTER];
        return entry.Set && entry.ZobristHash == hash && entry.VerifyDepthSet(depth);
    }

}