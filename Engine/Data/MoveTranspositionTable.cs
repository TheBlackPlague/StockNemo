using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Backend.Data.Struct;
using Engine.Data.Enum;
using Engine.Data.Struct;

namespace Engine.Data;

public unsafe class MoveTranspositionTable
{

    private const int MB_TO_B = 1_048_576;

    private readonly int HashFilter;
    private MoveTranspositionTableEntry[] Internal;

    public static MoveTranspositionTable GenerateTable(int megabyteSize) => new(megabyteSize * MB_TO_B);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public MoveTranspositionTable(int byteSize)
    {
        HashFilter = 0x0;

        for (int i = 0x1; byteSize >= (i + 1) * sizeof(MoveTranspositionTableEntry); i = (i << 1) | 0x1) {
            HashFilter = i;
        }

        int length = HashFilter + 1;
        Internal = new MoveTranspositionTableEntry[length];

        Parallel.For(0, length, i => Internal[i] = new MoveTranspositionTableEntry());

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
    public void InsertEntry(ulong zobristHash, MoveTranspositionTableEntryType type, SearchedMove bestMove, byte depth)
    {
        int index = (int)zobristHash & HashFilter;
        ref MoveTranspositionTableEntry entry = ref Internal[index];
        
        if (entry.Type == MoveTranspositionTableEntryType.Invalid) {
            entry.SetEntry(zobristHash, type, ref bestMove, depth);
            return;
        }
        
        if (entry.Depth - 3 > depth) return;
        entry.SetEntry(zobristHash, type, ref bestMove, depth);
    }

    public void FreeMemory() => Internal = null;

}