using System.Runtime.CompilerServices;
using Backend.Data.Template;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceColor Color<OriginalColor>() where OriginalColor : Color =>
        typeof(OriginalColor) == typeof(White) ? PieceColor.White : PieceColor.Black;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceColor OppositeColor<OriginalColor>() where OriginalColor : Color =>
        typeof(OriginalColor) == typeof(White) ? PieceColor.Black : PieceColor.White;

}