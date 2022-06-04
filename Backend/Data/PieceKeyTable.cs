using System;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data;

public class PieceKeyTable
{

    private readonly ulong[] Internal = new ulong[768];

    public PieceKeyTable(Random random)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        int i = 0;
        while (i < Internal.Length) {
            random.NextBytes(buffer);
            Internal[i++] = BitConverter.ToUInt64(buffer);
        }
    }

    public ulong this[Piece piece, PieceColor color, Square sq]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Internal[(int)color * 384 + (int)piece * 64 + (int)sq];
    }

}