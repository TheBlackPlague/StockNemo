using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend;

public static class Util
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceColor OppositeColor(PieceColor color) => (PieceColor)((int)color ^ 0x1);

}