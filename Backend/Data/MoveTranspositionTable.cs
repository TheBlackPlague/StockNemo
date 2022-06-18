#if DEBUG
using System;
#endif
using System;
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

        int length = HashFilter + 1;
        int partitionLength = length / (Environment.ProcessorCount * 2);
        Internal = new MoveTranspositionTableEntry[length];

        Parallel.For(0, Environment.ProcessorCount * 2, p =>
        {
            int start = p * partitionLength;
            int end = start + partitionLength;
            for (int i = start; i < end; i++) Internal[i] = new MoveTranspositionTableEntry();
        });

#if DEBUG
        Console.WriteLine("Allocated " + length * sizeof(MoveTranspositionTableEntry) + 
                          " bytes for " + length + " TT entries.");
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