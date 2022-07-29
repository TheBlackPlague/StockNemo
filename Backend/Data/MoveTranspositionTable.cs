#if DEBUG
using System;
#endif
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend.Data;

public unsafe class MoveTranspositionTable
{

    private const int MB_TO_B = 1_048_576;

    private readonly int HashFilter;
    private MoveTranspositionTableEntry[] Internal;

    public static MoveTranspositionTable GenerateTable(int megabyteSize) => new(megabyteSize * MB_TO_B);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private MoveTranspositionTable(int byteSize)
    {
        HashFilter = 0x0;

        for (int i = 0x1; byteSize >= (i + 1) * sizeof(MoveTranspositionTableEntry); i = (i << 1) | 0x1) {
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
        get => ref Internal.AA((int)zobristHash & HashFilter);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InsertEntry(ulong zobristHash, ref MoveTranspositionTableEntry entry)
    {
        int index = (int)zobristHash & HashFilter;
        ref MoveTranspositionTableEntry oldEntry = ref Internal.AA(index);
        
        if (oldEntry.Type == MoveTranspositionTableEntryType.Invalid) {
            // If the previous entry wasn't valid (there was no previous entry), replace it with the new entry. 
            Internal.AA(index) = entry;
            return;
        }
        
        // If the old entry is more than 3 depths higher than the new entry, than avoid replacing it.
        if (oldEntry.Depth - 3 > entry.Depth) return;
        Internal.AA(index) = entry;
    }

    public void FreeMemory() => Internal = null;

}