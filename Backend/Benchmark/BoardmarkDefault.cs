using Backend.Board;
using Backend.Move;
using BenchmarkDotNet.Attributes;

namespace Backend.Benchmark
{

    public class BoardmarkDefault
    {

        private readonly BitDataBoard Board = BitDataBoard.Default();

        [Benchmark]
        public BitDataBoard Clone() => Board.Clone();
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationPawn() => new(Board, (0, 1));
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationRook() => new(Board, (0, 0));
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationKnight() => new(Board, (1, 0));
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationBishop() => new(Board, (2, 0));
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationQueen() => new(Board, (3, 0));
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationKing() => new(Board, (4, 0));

    }

}