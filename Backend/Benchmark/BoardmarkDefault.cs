using Backend.Board;
using Backend.Move;
using BenchmarkDotNet.Attributes;

namespace Backend.Benchmark
{

    public class BoardmarkDefault
    {

        private readonly DataBoard Board = DataBoard.Default();

        [Benchmark]
        public DataBoard Clone() => Board.Clone();
        
        [Benchmark]
        public LegalMoveSet MoveGenerationPawn() => new(Board, (0, 1));
        
        [Benchmark]
        public LegalMoveSet MoveGenerationRook() => new(Board, (0, 0));
        
        [Benchmark]
        public LegalMoveSet MoveGenerationKnight() => new(Board, (1, 0));
        
        [Benchmark]
        public LegalMoveSet MoveGenerationBishop() => new(Board, (2, 0));
        
        [Benchmark]
        public LegalMoveSet MoveGenerationQueen() => new(Board, (3, 0));
        
        [Benchmark]
        public LegalMoveSet MoveGenerationKing() => new(Board, (4, 0));

    }

}