// using Backend;
// using Backend.Data.Enum;
// using Backend.Data.Move;
// using Backend.Data.Struct;
// using BenchmarkDotNet.Attributes;
//
// namespace Benchmark.Move
// {
//
//     public class LegalMoveSetMark
//     {
//
//         private readonly Backend.Board Board = Backend.Board.FromFen("8/2Q5/5B2/8/1N2R3/7P/3K4/8 w - - 0 1");
//
//         [Benchmark]
//         public Backend.Board Clone() => Board.Clone();
//         
//         [Benchmark]
//         public MoveList MoveGenerationPawn() => new(Board, (7, 2), false);
//         
//         [Benchmark]
//         public MoveList MoveGenerationRook() => new(Board, (4, 3), false);
//         
//         [Benchmark]
//         public MoveList MoveGenerationKnight() => new(Board, (1, 3), false);
//         
//         [Benchmark]
//         public MoveList MoveGenerationBishop() => new(Board, (5, 5), false);
//         
//         [Benchmark]
//         public MoveList MoveGenerationQueen() => new(Board, (2, 6), false);
//         
//         [Benchmark]
//         public MoveList MoveGenerationKing() => new(Board, (3, 1), false);
//
//         [Benchmark]
//         public bool UnderAttack() => MoveList.UnderAttack(Board, (3, 1), PieceColor.Black);
//
//     }
//
// }