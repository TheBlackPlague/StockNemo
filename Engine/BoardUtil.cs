using System.Runtime.CompilerServices;
using Backend;
using Backend.Data.Struct;
using Engine.Data.Struct;

namespace Engine;

public static class BoardUtil
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RevertMove Move(Board board, ref OrderedMoveEntry move) => 
        board.Move(move.From, move.To, move.Promotion);

}