using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend.Engine;

public static class SEE
{

    private static readonly int[] Internal = { 82, 477, 337, 365, 1025, 0, 0 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Approximate(EngineBoard board, ref OrderedMoveEntry move)
    {
        Piece from = board.PieceOnly(move.From);
        Piece to = board.PieceOnly(move.To);
        
        // In case of En Passant, we set the target piece to a pawn.
        if (from == Piece.Pawn && move.To == board.EnPassantTarget) to = Piece.Pawn;

        int value = Internal.AA((int)to);
        
        if (move.Promotion != Promotion.None)
            // In the case of a promotion, increment with the difference of the promotion and pawn.
            value += Internal.AA((int)move.Promotion) - Internal.AA((int)Piece.Pawn);

        return value - Internal.AA((int)from);
    }

}