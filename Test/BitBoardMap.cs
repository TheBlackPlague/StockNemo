using System;
using Backend.Data.Enum;
using Backend.Data.Struct;
using Backend.Data.Template;
using NUnit.Framework;

namespace Test;

public class BitBoardMap
{

    private readonly Backend.Data.Struct.BitBoardMap Map = 
        new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", "w", "KQkq", "-");

    [Test]
    public void Clone() => Assert.AreEqual(Map, Map);
        
    [Test]
    public void GetWhitePawn() => Assert.AreEqual((Piece.Pawn, PieceColor.White), Map[Square.A2]);
        
    [Test]
    public void GetWhiteRook() => Assert.AreEqual((Piece.Rook, PieceColor.White), Map[Square.A1]);
        
    [Test]
    public void GetWhiteQueen() => Assert.AreEqual((Piece.Queen, PieceColor.White), Map[Square.D1]);
        
    [Test]
    public void GetWhiteKing() => Assert.AreEqual((Piece.King, PieceColor.White), Map[Square.E1]);
        
    [Test]
    public void GetBlackPawn() => Assert.AreEqual((Piece.Pawn, PieceColor.Black), Map[Square.A7]);
        
    [Test]
    public void GetBlackRook() => Assert.AreEqual((Piece.Rook, PieceColor.Black), Map[Square.A8]);
        
    [Test]
    public void GetBlackQueen() => Assert.AreEqual((Piece.Queen, PieceColor.Black), Map[Square.D8]);
        
    [Test]
    public void GetBlackKing() => Assert.AreEqual((Piece.King, PieceColor.Black), Map[Square.E8]);

    [Test]
    public void MoveWhitePawn()
    {
        Backend.Data.Struct.BitBoardMap useMap = Map.Copy();
            
        useMap.Move<Normal>(Square.A2, Square.A4);
        Assert.AreEqual((Piece.Pawn, PieceColor.White), useMap[Square.A4]);
    }
        
    [Test]
    public void MoveWhitePawnInEnemy()
    {
        Backend.Data.Struct.BitBoardMap useMap = Map.Copy();
            
        useMap.Move<Normal>(Square.A2, Square.A7);

        bool success = (Piece.Pawn, PieceColor.White) == useMap[Square.A7] &&
                       useMap[Piece.Pawn, PieceColor.Black].Count == 7;
        Assert.IsTrue(success);
    }
        
    [Test]
    public void RemoveWhitePawn()
    {
        Backend.Data.Struct.BitBoardMap useMap = Map.Copy();
            
        useMap.Empty<Normal>(Square.A2);
        Assert.AreEqual((Piece.Empty, PieceColor.None), useMap[Square.A2]);
    }
        
    [Test]
    public void MoveKnightToA3()
    {
        Backend.Data.Struct.BitBoardMap useMap = Map;
            
        useMap.Move<Normal>(Square.B1, Square.A3);
        Assert.AreEqual((Piece.Knight, PieceColor.White), useMap[Square.A3]);
    }

    [Test]
    public void ConfirmBoardState()
    {
        string fen = Map.GenerateBoardFen();
        Console.WriteLine(fen);
        Assert.AreEqual("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", fen);
    }

    [Test]
    public void UndoEval()
    {
        Backend.Data.Struct.BitBoardMap useMap = Map;

        int eval = useMap.MaterialDevelopmentEvaluationEarly;
        Console.WriteLine("Previous Eval: " + eval);
        useMap.Move<ClassicalUpdate>(Square.E2, Square.E4);
        int newEval = useMap.MaterialDevelopmentEvaluationEarly;
        Console.WriteLine("New Eval: " + newEval);
        bool evalChanged = eval != newEval;
        useMap.Move<ClassicalUpdate>(Square.E4, Square.E2);
        int prevEval = useMap.MaterialDevelopmentEvaluationEarly;
        Console.WriteLine("Reverted Eval: " + prevEval);
        bool evalReverted = eval == prevEval;
        
        Assert.IsTrue(evalChanged && evalReverted);
    }

}