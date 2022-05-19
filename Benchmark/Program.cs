using Backend.Data.Move;
using Backend.Data.Struct;
using Benchmark.Move;
using BenchmarkDotNet.Running;

namespace Benchmark
{

    public static class Program
    {

        public static void Main()
        {
            MoveList.SetUp();
            
            // BenchmarkRunner.Run<BoardmarkDefault>();
            // BenchmarkRunner.Run<BoardMarkKiwipete>();
            // BenchmarkRunner.Run<BitBoardMapMarkDefault>();
            BenchmarkRunner.Run<LegalMoveSetMark>();
        }

    }

}