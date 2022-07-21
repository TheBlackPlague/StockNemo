using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Backend.Data.Enum;

namespace Backend.Data.Struct;

[StructLayout(LayoutKind.Sequential)]
public struct MoveTranspositionTableEntry
{
    
    public ulong ZobristHash { get; }
    public MoveTranspositionTableEntryType Type { get; }
    public SearchedMove BestMove { get; }
    public byte Depth { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveTranspositionTableEntry()
    {
        ZobristHash = 0UL;
        Type = MoveTranspositionTableEntryType.Invalid;
        BestMove = new SearchedMove();
        Depth = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveTranspositionTableEntry(
        ulong zobristHash, MoveTranspositionTableEntryType type, SearchedMove bestMove, int depth
    )
    {
        ZobristHash = zobristHash;
        Type = type;
        BestMove = bestMove;
        Depth = (byte)depth;
    }

}