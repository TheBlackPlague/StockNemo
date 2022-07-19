using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data;

public class HistoryTable
{

    private const int SIZE = 2 * 6 * 64;

    private readonly int[] Internal = new int[SIZE];

    public int this[Piece piece, PieceColor color, Square targetSq]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Internal.AA((int)color * 384 + (int)piece * 64 + (int)targetSq);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Internal.AA((int)color * 384 + (int)piece * 64 + (int)targetSq) = value;
    }

}