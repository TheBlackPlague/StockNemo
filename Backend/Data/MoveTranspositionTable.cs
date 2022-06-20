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

        Internal = new MoveTranspositionTableEntry[HashFilter];

        Parallel.For(0, HashFilter, i => Internal[i] = new MoveTranspositionTableEntry());

#if DEBUG
        Console.WriteLine("Allocated " + HashFilter * sizeof(MoveTranspositionTableEntry) + 
                          " bytes for " + HashFilter + " TT entries.");
#endif
    }

    public ref MoveTranspositionTableEntry this[ulong zobristHash]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Internal[(int)zobristHash & HashFilter];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InsertEntry(ulong zobristHash, ref MoveTranspositionTableEntry entry)
    {
        int index = (int)zobristHash & HashFilter;
        ref MoveTranspositionTableEntry oldEntry = ref Internal[index];
        
        if (oldEntry.Type == MoveTranspositionTableEntryType.Invalid) {
            Internal[index] = entry;
            return;
        }
        
        if (oldEntry.Depth - 3 > entry.Depth) return;
        Internal[index] = entry;
    }

    public void FreeMemory() => Internal = null;

}