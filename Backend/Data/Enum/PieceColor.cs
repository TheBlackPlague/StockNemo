using System.Runtime.CompilerServices;

namespace Backend.Data.Enum;

public enum PieceColor : byte
{
        
    // The color of the piece.

    White,
    Black,
    None

}

public static class PieceColorUtil
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceColor OppositeColor(this PieceColor color) => (PieceColor)((int)color ^ 0x1);

}