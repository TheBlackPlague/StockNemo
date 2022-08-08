using Backend;
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
    public void EvaluateBasic() => BasicNNUE.Evaluate(Board.ColorToMove);

}