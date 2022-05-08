using Backend.Board;
using Backend.Move;
using BenchmarkDotNet.Attributes;

namespace Backend.Benchmark
{

    public class BoardMarkKiwipete
    {

        private readonly DataBoard Board = 
            DataBoard.FromFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -");

        [Benchmark]
        public DataBoard Clone() => Board.Clone();
        
        [Benchmark]
        public LegalMoveSet MoveGenerationPawn() => new(Board, (6, 1));
        
        [Benchmark]
        public LegalMoveSet MoveGenerationRook() => new(Board, (0, 0));
        
        [Benchmark]
        public LegalMoveSet MoveGenerationKnight() => new(Board, (4, 4));
        
        [Benchmark]
        public LegalMoveSet MoveGenerationBishop() => new(Board, (4, 1));
        
        [Benchmark]
        public LegalMoveSet MoveGenerationQueen() => new(Board, (5, 2));
        
        [Benchmark]
        public LegalMoveSet MoveGenerationKing() => new(Board, (4, 0));

    }

}