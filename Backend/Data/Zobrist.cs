using System;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend.Data;

public static class Zobrist
{

    private static readonly Random Random = new();
    private static readonly ZobristPieceKeyTable PieceKeys = new(Random);
    private static readonly ulong[] CastlingKeys = new ulong[16];
    private static readonly ulong[] EnPassantKeys = new ulong[64];
    private static ulong TurnKey;

    public static void Setup()
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];

        for (byte i = 0; i < CastlingKeys.Length; i++) {
            Random.NextBytes(buffer);
            CastlingKeys[i] = BitConverter.ToUInt64(buffer);
        }

        for (int i = 0; i < EnPassantKeys.Length; i++) {
            Random.NextBytes(buffer);
            EnPassantKeys[i] = BitConverter.ToUInt64(buffer);
        }

        TurnKey = BitConverter.ToUInt64(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static ulong Hash(ref BitBoardMap map)
    {
        ulong zobristHash = 0UL;
        Piece piece = Piece.Pawn;
        PieceColor color = map.WhiteTurn ? PieceColor.White : PieceColor.Black;
        while (piece < Piece.Empty) {
            BitBoardIterator pieceSquareIterator = map[piece, color].GetEnumerator();
            Square sq = pieceSquareIterator.Current;
            while (pieceSquareIterator.MoveNext()) {
                zobristHash ^= PieceKeys[piece, color, sq];
                sq = pieceSquareIterator.Current;
            }
            
            piece++;
        }

        if (map.WhiteTurn) zobristHash ^= TurnKey;
        if (map.EnPassantTarget != Square.Na) zobristHash ^= EnPassantKeys[(int)map.EnPassantTarget];
        
        zobristHash ^= CastlingKeys[map.WhiteKCastle | map.WhiteQCastle | map.BlackKCastle | map.BlackQCastle];

        return zobristHash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void HashPiece(ref ulong zobristHash, Piece piece, PieceColor color, Square sq) => 
        zobristHash ^= PieceKeys[piece, color, sq];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void HashCastlingRights(ref ulong zobristHash, byte wk, byte wq, byte bk, byte bq) => 
        zobristHash ^= CastlingKeys[wk | wq | bk | bq];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FlipTurnInHash(ref ulong zobristHash) => zobristHash ^= TurnKey;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void HashEp(ref ulong zobristHash, Square ep) => zobristHash ^= EnPassantKeys[(int)ep];

}