#if DEBUG
using System;
#endif
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Backend.Data.Enum;
using Backend.Data.Struct;
using Backend.Data.Template;

namespace Backend.Data;

public unsafe class MoveTranspositionTable
{

    private const int MB_TO_B = 1_048_576;

    private const int REPLACEMENT_DEPTH_THRESHOLD = 3;

    private readonly int HashFilter;
    private MoveTranspositionTableEntry[] Internal;

    public static MoveTranspositionTable GenerateTable(int megabyteSize) => new(megabyteSize * MB_TO_B);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private MoveTranspositionTable(int byteSize)
    {
        HashFilter = 0x0;

        for (int i = 0x1; byteSize >= (i + 1) * sizeof(MoveTranspositionTableEntry); i = i << 1 | 0x1) {
            HashFilter = i;
        }

        Internal = new MoveTranspositionTableEntry[HashFilter + 1];

        Parallel.For(0, HashFilter + 1, i => 
            Internal[i] = new MoveTranspositionTableEntry()
        );

#if DEBUG
        Console.WriteLine("Allocated " + HashFilter * sizeof(MoveTranspositionTableEntry) + 
                          " bytes for " + HashFilter + " TT entries.");
#endif
    }

    public ref MoveTranspositionTableEntry this[ulong zobristHash]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref this[(int)zobristHash & HashFilter];
    }

    public ref MoveTranspositionTableEntry this[int transpositionKey]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Internal.AA(transpositionKey);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InsertEntry(ref MoveTranspositionTableEntry entry)
    {
        int index = (int)entry.ZobristHash & HashFilter;
        ref MoveTranspositionTableEntry oldEntry = ref Internal.AA(index);

        // Replace Scheme:
        // - ENTRY_TYPE == EXACT
        // - OLD_ENTRY_HASH != NEW_ENTRY_HASH
        // - OLD_ENTRY_TYPE == ALPHA_UNCHANGED && ENTRY_TYPE == BETA_CUTOFF
        // - ENTRY_DEPTH > OLD_ENTRY_DEPTH - REPLACEMENT_THRESHOLD
        if (entry.Type == MoveTranspositionTableEntryType.Exact || entry.ZobristHash != oldEntry.ZobristHash || 
            oldEntry.Type == MoveTranspositionTableEntryType.AlphaUnchanged && 
            entry.Type == MoveTranspositionTableEntryType.BetaCutoff ||
            entry.Depth > oldEntry.Depth - REPLACEMENT_DEPTH_THRESHOLD)
            Internal.AA(index) = entry;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Prefetch(ulong zobristHash)
    {
        int index = (int)zobristHash & HashFilter;
        Internal.Prefetch<MoveTranspositionTableEntry, L1>(index);
        return index;
    }

    public void FreeMemory() => Internal = null;
    
}
