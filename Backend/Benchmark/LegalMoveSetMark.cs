using Backend.Board;
using Backend.Move;
using BenchmarkDotNet.Attributes;

namespace Backend.Benchmark
{

    public class LegalMoveSetMark
    {

        private readonly DataBoard Board = DataBoard.FromFen("8/2Q5/5B2/8/1N2R3/7P/3K4/8 w - - 0 1");

        [Benchmark]
        public DataBoard Clone() => Board.Clone();
        
        [Benchmark]
        public LegalMoveSet MoveGenerationPawn() => new(Board, (7, 2), false);
        
        [Benchmark]
        public LegalMoveSet MoveGenerationRook() => new(Board, (4, 3), false);
        
        [Benchmark]
        public LegalMoveSet MoveGenerationKnight() => new(Board, (1, 3), false);
        
        [Benchmark]
        public LegalMoveSet MoveGenerationBishop() => new(Board, (5, 5), false);
        
        [Benchmark]
        public LegalMoveSet MoveGenerationQueen() => new(Board, (2, 6), false);
        
        [Benchmark]
        public LegalMoveSet MoveGenerationKing() => new(Board, (3, 1), false);

        [Benchmark]
        public bool UnderAttack() => LegalMoveSet.UnderAttack(Board, (3, 1), PieceColor.Black);

    }

}