using Backend;
using Backend.Data.Enum;
using Backend.Engine.NNUE.Architecture.Basic;
using NUnit.Framework;

namespace Test;

public class NNUE
{
    
    private static readonly Board Board = Board.Default();
    private static readonly BasicNNUE BasicNNUE = new();

    [Test]
    public void RefreshBasicAccumulator() => BasicNNUE.RefreshAccumulator(Board);

    [Test]
    public void Eua()
    {
        BasicNNUE.EfficientlyUpdateAccumulator(Piece.Pawn, PieceColor.White, Square.E2, false);
        BasicNNUE.EfficientlyUpdateAccumulator(Piece.Pawn, PieceColor.White, Square.E4);
    }
    
    [Test]
    public void EvaluateBasic() => BasicNNUE.Evaluate(Board.ColorToMove);

}