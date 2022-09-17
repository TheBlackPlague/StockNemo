using Backend;
using Backend.Data.Enum;
using Backend.Data.Template;
using Backend.Engine.NNUE.Architecture.Basic;
using BenchmarkDotNet.Attributes;

namespace Benchmark;

[DisassemblyDiagnoser(10)]
public class NNUE
{

    private static readonly Board Board = Board.Default();
    private static readonly BasicNNUE BasicNNUE = new();

    // [Benchmark]
    // public void RefreshBasicAccumulator() => BasicNNUE.RefreshAccumulator(Board);
    //
    // [Benchmark]
    // public void PushPullAccumulator()
    // {
    //     BasicNNUE.PushAccumulator();
    //     BasicNNUE.PullAccumulator();
    // }
    //
    // [Benchmark]
    // public void EuaNormal()
    // {
    //     BasicNNUE.EfficientlyUpdateAccumulator(Piece.Pawn, PieceColor.White, Square.E2, Square.E4);
    // }
    //
    // [Benchmark]
    // public void EuaGeneric()
    // {
    //     BasicNNUE.EfficientlyUpdateAccumulator<Deactivate>(Piece.Pawn, PieceColor.White, Square.E2);
    //     BasicNNUE.EfficientlyUpdateAccumulator<Activate>(Piece.Pawn, PieceColor.White, Square.E4);
    // }
    //
    // [Benchmark]
    // public void EvaluateBasic() => BasicNNUE.Evaluate(Board.ColorToMove);

}