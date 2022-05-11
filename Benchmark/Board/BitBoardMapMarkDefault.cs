using Backend.Board;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Board
{

    public class BitBoardMapMarkDefault
    {

        private readonly BitBoardMap Map = new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");

        [Benchmark]
        public BitBoardMap Clone() => Map;
        
        [Benchmark]
        public (Piece, PieceColor) GetWhitePawn() => Map[0, 1];
        
        [Benchmark]
        public (Piece, PieceColor) GetWhiteRook() => Map[0, 0];
        
        [Benchmark]
        public (Piece, PieceColor) GetWhiteQueen() => Map[3, 0];
        
        [Benchmark]
        public (Piece, PieceColor) GetWhiteKing() => Map[4, 0];
        
        [Benchmark]
        public (Piece, PieceColor) GetBlackPawn() => Map[0, 6];
        
        [Benchmark]
        public (Piece, PieceColor) GetBlackRook() => Map[0, 7];
        
        [Benchmark]
        public (Piece, PieceColor) GetBlackQueen() => Map[3, 7];
        
        [Benchmark]
        public (Piece, PieceColor) GetBlackKing() => Map[4, 7];

    }

}