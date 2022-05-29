using Backend;
using Backend.Data.Enum;
using BenchmarkDotNet.Attributes;

namespace Benchmark;

public class MoveList
{

    private readonly Board Board = Board.FromFen("8/2Q5/5B2/8/1N2R3/7P/3K4/8 w - - 0 1");

    [Benchmark]
    public Board Clone() => Board.Clone();
        
    [Benchmark]
    public Backend.Data.Struct.MoveList MoveGenerationPawn() => 
        Backend.Data.Struct.MoveList.WithoutProvidedPins(Board, Square.H3);
        
    [Benchmark]
    public Backend.Data.Struct.MoveList MoveGenerationRook() => 
        Backend.Data.Struct.MoveList.WithoutProvidedPins(Board, Square.E4);
        
    [Benchmark]
    public Backend.Data.Struct.MoveList MoveGenerationKnight() => 
        Backend.Data.Struct.MoveList.WithoutProvidedPins(Board, Square.B4);
        
    [Benchmark]
    public Backend.Data.Struct.MoveList MoveGenerationBishop() => 
        Backend.Data.Struct.MoveList.WithoutProvidedPins(Board, Square.F6);
        
    [Benchmark]
    public Backend.Data.Struct.MoveList MoveGenerationQueen() => 
        Backend.Data.Struct.MoveList.WithoutProvidedPins(Board, Square.C7);
        
    [Benchmark]
    public Backend.Data.Struct.MoveList MoveGenerationKing() => 
        Backend.Data.Struct.MoveList.WithoutProvidedPins(Board, Square.D2);

    [Benchmark]
    public bool UnderAttack() => Backend.Data.Struct.MoveList.UnderAttack(Board, Square.D2, PieceColor.Black);

}