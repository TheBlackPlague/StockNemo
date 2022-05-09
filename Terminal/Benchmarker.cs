using Backend.Benchmark;
using Backend.Move;
using BenchmarkDotNet.Running;

namespace Terminal
{

    public class Benchmarker
    {

        public static void RunAll()
        {
            LegalMoveSet.SetUp();
            
            // BenchmarkRunner.Run<BoardmarkDefault>();
            // BenchmarkRunner.Run<BoardMarkKiwipete>();
            // BenchmarkRunner.Run<BitBoardMapMarkDefault>();
            BenchmarkRunner.Run<LegalMoveSetMark>();
        }

    }

}