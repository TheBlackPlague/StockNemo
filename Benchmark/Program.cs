using Backend;
using Backend.Data.Move;
using BenchmarkDotNet.Running;

namespace Benchmark;

public static class Program
{

    public static void Main()
    {
        PreMark();
        BitBoardMapRunner();
        MoveListRunner();
        PerftRunner();
    }

    public static void BitBoardMapRunner()
    {
        PreMark();
        BenchmarkRunner.Run<BitBoardMap>();
    }

    public static void MoveListRunner()
    {
        PreMark();
        BenchmarkRunner.Run<MoveList>();
    }
        
    public static void PerftRunner()
    {
        PreMark();
        BenchmarkRunner.Run<Perft>();
    }

    private static void PreMark()
    {
        Util.RunStaticConstructor();
        AttackTable.SetUp();
    }

}