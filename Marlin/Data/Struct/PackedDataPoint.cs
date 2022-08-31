using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Data.Struct;
using Marlin.Data.Enum;

namespace Marlin.Data.Struct;

public struct PackedDataPoint
{

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly BitBoard Occupied;
    private readonly PackedPieceArray PieceArray;
    private readonly int Evaluation;
    private readonly PieceColor ColorToMove;
    private readonly WDL WDL;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public PackedDataPoint(ReadOnlySpan<char> fen, int evaluation, PieceColor colorToMove, WDL wdl)
    {
        Occupied = BitBoard.Default;
        PackedPieceArrayGenerator generator = default;

        for (int v = 0; v < 8; v++) {
            ReadOnlySpan<char> rankData = fen.Until('/');
            int h = 0;
            foreach (char ch in rankData) {
                if (char.IsNumber(ch)) {
                    h += ch - '0';
                    continue;
                }

                Piece piece = Piece.Pawn;
                PieceColor color = PieceColor.White;
                switch (ch) {
                    case 'r':
                    case 'R':
                        piece = Piece.Rook;
                        break;
                    case 'n':
                    case 'N':
                        piece = Piece.Knight;
                        break;
                    case 'b':
                    case 'B':
                        piece = Piece.Bishop;
                        break;
                    case 'q':
                    case 'Q':
                        piece = Piece.Queen;
                        break;
                    case 'k':
                    case 'K':
                        piece = Piece.King;
                        break;
                }
                if (char.IsLower(ch)) color = PieceColor.Black;

                Occupied[v * 8 + h] = true;
                generator.AddPiece(piece, color);
                h++;
            }
            fen = fen[rankData.Length..];
        }

        PieceArray = generator.Internal;

        ColorToMove = colorToMove;
        Evaluation = evaluation;
        WDL = wdl;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteToBinary(Stream stream)
    {
        Span<byte> temp = stackalloc byte[
            Unsafe.SizeOf<BitBoard>() + Unsafe.SizeOf<PackedPieceArray>() + Unsafe.SizeOf<int>() + 
            Unsafe.SizeOf<PieceColor>() + Unsafe.SizeOf<WDL>()
        ];
        
        Unsafe.WriteUnaligned(ref temp[0], this);
        stream.Write(temp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadFromBinary(Stream stream)
    {
        Span<byte> temp = stackalloc byte[
            Unsafe.SizeOf<BitBoard>() + Unsafe.SizeOf<PackedPieceArray>() + Unsafe.SizeOf<int>() + 
            Unsafe.SizeOf<PieceColor>() + Unsafe.SizeOf<WDL>()
        ];

        int n = stream.Read(temp);
        if (n < temp.Length) throw new InvalidDataException("Not enough data to read.");
        
        this = Unsafe.ReadUnaligned<PackedDataPoint>(ref temp[0]);
    }

}