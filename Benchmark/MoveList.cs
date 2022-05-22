using Backend.Data.Enum;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{

    public class MoveList
    {

        private readonly Backend.Board Board = Backend.Board.FromFen("8/2Q5/5B2/8/1N2R3/7P/3K4/8 w - - 0 1");

        [Benchmark]
        public Backend.Board Clone() => Board.Clone();
        
        [Benchmark]
        public Backend.Data.Struct.MoveList MoveGenerationPawn() => new(Board, Square.H3, false);
        
        [Benchmark]
        public Backend.Data.Struct.MoveList MoveGenerationRook() => new(Board, Square.E4, false);
        
        [Benchmark]
        public Backend.Data.Struct.MoveList MoveGenerationKnight() => new(Board, Square.B4, false);
        
        [Benchmark]
        public Backend.Data.Struct.MoveList MoveGenerationBishop() => new(Board, Square.F6, false);
        
        [Benchmark]
        public Backend.Data.Struct.MoveList MoveGenerationQueen() => new(Board, Square.C7, false);
        
        [Benchmark]
        public Backend.Data.Struct.MoveList MoveGenerationKing() => new(Board, Square.D2, false);

        [Benchmark]
        public bool UnderAttack() => Backend.Data.Struct.MoveList.UnderAttack(Board, Square.D2, PieceColor.Black);

    }

}