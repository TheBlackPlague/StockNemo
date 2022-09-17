using Backend;
using Backend.Data.Enum;
using Backend.Data.Struct;
using BenchmarkDotNet.Attributes;

namespace Benchmark;

[DisassemblyDiagnoser(maxDepth: 10)]
public class MoveList
{

    private readonly Board Board = Board.FromFen("8/2Q5/5B2/8/1N2R3/7P/3K4/8 w - - 0 1");
    private BitBoard C;
    private bool DoubleChecked;
    private BitBoard D;
    private BitBoard Hv;

    // [GlobalSetup]
    // public void Setup()
    // {
    //     (C, DoubleChecked) = Backend.Data.Struct.MoveList.CheckBitBoard(
    //         Board, 
    //         Board.KingLoc(PieceColor.White), 
    //         PieceColor.Black
    //     );
    //     (Hv, D) = Backend.Data.Struct.MoveList.PinBitBoards(
    //         Board, 
    //         Board.KingLoc(PieceColor.White), 
    //         PieceColor.White,
    //         PieceColor.Black
    //     );
    // }
    //
    // [Benchmark]
    // public Board Clone() => Board.Clone();
    //
    // [Benchmark]
    // public Backend.Data.Struct.MoveList MoveGenerationPawn() => 
    //     new(Board, Square.H3, Piece.Pawn, PieceColor.White, 
    //         ref Hv, ref D, ref C, DoubleChecked
    //     );
    //
    // [Benchmark]
    // public Backend.Data.Struct.MoveList MoveGenerationRook() => 
    //     new(Board, Square.E4, Piece.Rook, PieceColor.White, 
    //         ref Hv, ref D, ref C, DoubleChecked
    //     );
    //     
    // [Benchmark]
    // public Backend.Data.Struct.MoveList MoveGenerationKnight() => 
    //     new(Board, Square.B4, Piece.Knight, PieceColor.White, 
    //         ref Hv, ref D, ref C, DoubleChecked
    //     );
    //     
    // [Benchmark]
    // public Backend.Data.Struct.MoveList MoveGenerationBishop() => 
    //     new(Board, Square.F6, Piece.Bishop, PieceColor.White, 
    //         ref Hv, ref D, ref C, DoubleChecked
    //     );
    //     
    // [Benchmark]
    // public Backend.Data.Struct.MoveList MoveGenerationQueen() => 
    //     new(Board, Square.C7, Piece.Queen, PieceColor.White, 
    //         ref Hv, ref D, ref C, DoubleChecked
    //     );
    //     
    // [Benchmark]
    // public Backend.Data.Struct.MoveList MoveGenerationKing() => 
    //     new(Board, Square.D2, Piece.King, PieceColor.White, 
    //         ref Hv, ref D, ref C, DoubleChecked
    //     );
    //
    // [Benchmark]
    // public bool UnderAttack() => Backend.Data.Struct.MoveList.UnderAttack(Board, Square.D2, PieceColor.Black);

}