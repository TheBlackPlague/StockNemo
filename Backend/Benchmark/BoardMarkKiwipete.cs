using Backend.Board;
using Backend.Move;
using BenchmarkDotNet.Attributes;

namespace Backend.Benchmark
{

    public class BoardMarkKiwipete
    {

        private readonly BitDataBoard Board = 
            BitDataBoard.FromFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -");

        [Benchmark]
        public BitDataBoard Clone() => Board.Clone();
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationPawn() => new(Board, (6, 1));
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationRook() => new(Board, (0, 0));
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationKnight() => new(Board, (4, 4));
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationBishop() => new(Board, (4, 1));
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationQueen() => new(Board, (5, 2));
        
        [Benchmark]
        public BitLegalMoveSet MoveGenerationKing() => new(Board, (4, 0));

    }

}