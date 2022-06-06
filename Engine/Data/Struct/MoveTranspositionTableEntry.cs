using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Engine.Data.Enum;

namespace Engine.Data.Struct;

[StructLayout(LayoutKind.Sequential)]
public struct MoveTranspositionTableEntry
{

    public static MoveTranspositionTableEntry Default = new();
    
    public ulong ZobristHash { get; private set; }
    public MoveTranspositionTableEntryType Type { get; private set; }
    public SearchedMove BestMove { get; private set; }
    public byte Depth { get; private set; }

    public MoveTranspositionTableEntry()
    {
        ZobristHash = 0UL;
        Type = MoveTranspositionTableEntryType.Invalid;
        BestMove = new SearchedMove();
        Depth = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetEntry(
        ulong zobristHash, MoveTranspositionTableEntryType type, ref SearchedMove bestMove, byte depth
    )
    {
        ZobristHash = zobristHash;
        Type = type;
        BestMove = bestMove;
        Depth = depth;
    }

}