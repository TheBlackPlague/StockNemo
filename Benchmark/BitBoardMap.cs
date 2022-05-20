using Backend.Data.Enum;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{

    public class BitBoardMap
    {

        private readonly Backend.Data.Struct.BitBoardMap Map = 
            new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", "w", "KQkq", "-");

        [Benchmark]
        public Backend.Data.Struct.BitBoardMap Clone() => Map;
        
        [Benchmark]
        public (Piece, PieceColor) GetWhitePawn() => Map[Square.A2];
        
        [Benchmark]
        public (Piece, PieceColor) GetWhiteRook() => Map[Square.A1];
        
        [Benchmark]
        public (Piece, PieceColor) GetWhiteQueen() => Map[Square.D1];
        
        [Benchmark]
        public (Piece, PieceColor) GetWhiteKing() => Map[Square.E1];
        
        [Benchmark]
        public (Piece, PieceColor) GetBlackPawn() => Map[Square.A7];
        
        [Benchmark]
        public (Piece, PieceColor) GetBlackRook() => Map[Square.A8];
        
        [Benchmark]
        public (Piece, PieceColor) GetBlackQueen() => Map[Square.D8];
        
        [Benchmark]
        public (Piece, PieceColor) GetBlackKing() => Map[Square.E8];

    }

}