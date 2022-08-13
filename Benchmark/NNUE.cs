using Backend;
using Backend.Data.Enum;
using Backend.Engine.NNUE.Architecture.Basic;
using BenchmarkDotNet.Attributes;

namespace Benchmark;

[DisassemblyDiagnoser(10)]
public class NNUE
{

    private static readonly Board Board = Board.Default();
    private static readonly BasicNNUE BasicNNUE = new();

    [Benchmark]
    public void RefreshBasicAccumulator() => BasicNNUE.RefreshAccumulator(Board);

    [Benchmark]
    public void PushPullAccumulator()
    {
        BasicNNUE.PushAccumulator();
        BasicNNUE.PullAccumulator();
    }

    [Benchmark]
    public void Eua()
    {
        BasicNNUE.EfficientlyUpdateAccumulator(Piece.Pawn, PieceColor.White, Square.E2, false);
        BasicNNUE.EfficientlyUpdateAccumulator(Piece.Pawn, PieceColor.White, Square.E4);
    }

    [Benchmark]
    public void EvaluateBasic() => BasicNNUE.Evaluate(Board.ColorToMove);

}