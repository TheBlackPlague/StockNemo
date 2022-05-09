using Backend.Move;
using Benchmark.Move;
using BenchmarkDotNet.Running;

namespace Benchmark
{

    public static class Program
    {

        public static void Main()
        {
            LegalMoveSet.SetUp();
            
            // BenchmarkRunner.Run<BoardmarkDefault>();
            // BenchmarkRunner.Run<BoardMarkKiwipete>();
            // BenchmarkRunner.Run<BitBoardMapMarkDefault>();
            BenchmarkRunner.Run<LegalMoveSetMark>();
        }

    }

}