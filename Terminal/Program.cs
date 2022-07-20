using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Backend;
using Backend.Data;
using Backend.Data.Move;
using Terminal.Interactive;
using Terminal.Uci;

namespace Terminal;

internal static class Program
{

    private static PerftTranspositionTable Table;

    private static void Main()
    {
        Util.RunStaticConstructor();
        
        TaskFactory factory = new();
        Task hardwareInitializationTask = factory.StartNew(HardwareInitializer.Setup);
        
        AttackTable.SetUp();
        Zobrist.Setup();
        
        // Run JIT.
        Perft.MoveGeneration(Board.Default(), 5, false);

        string command = Environment.CommandLine;

        if (command.ToLower().Contains("--uci=True")) {
            goto MainInput;
        }
        
        if (command.ToLower().Contains("--perft-tt=true")) {
            Table = new PerftTranspositionTable();
        }

        if (command.ToLower().Contains("bench")) {
            OpenBenchBenchmark.Bench();
            return;
        }
        
        DrawCycle.OutputTitle();

        MainInput:
        string[] args = Console.ReadLine()?.Split(" ");
        
        if (args == null) goto MainInput;

        switch (args[0].ToLower()) {
            case "uci":
                StreamWriter standardOutput = new(Console.OpenStandardOutput());
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
                UniversalChessInterface.Setup();
                UniversalChessInterface.LaunchUci();
                return;
            case "interactive":
                hardwareInitializationTask.Wait();
                InteractiveInterface.Start();
                return;
            default:
                goto MainInput;
        }
    }

    [SuppressMessage("ReSharper", "UseStringInterpolation")]
    public static void RunPerft(Board board, int depth = 1)
    {
        Console.WriteLine("Running PERFT @ depth " + depth + ": ");
        
        Stopwatch watch = new();
        ulong result;
        if (Table != null) {
            Table.HitCount = 0;
            watch.Start();
            result = Perft.MoveGeneration(board, depth, Table);
            watch.Stop();
        } else {
            watch.Start();
            result = Perft.MoveGeneration(board, depth);
            watch.Stop();
        }
            
        string output = "Searched " + result.ToString("N0") + " nodes (" + watch.ElapsedMilliseconds + " ms).";
        string ttHitResult = "";
        if (Table != null) {
            ttHitResult = " TT: " + Table.HitCount + " hits.";
        }
        Console.WriteLine(output + ttHitResult);
    }

}