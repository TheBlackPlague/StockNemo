using Backend.Benchmark;
using BenchmarkDotNet.Running;

namespace Terminal
{

    public class Benchmarker
    {

        public static void RunAll()
        {
            BenchmarkRunner.Run<BoardmarkDefault>();
            BenchmarkRunner.Run<BoardMarkKiwipete>();
        }

    }

}