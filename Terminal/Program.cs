using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Backend;
using Backend.Data.Enum;
using Backend.Data.Move;
using Version = Backend.Version;

namespace Terminal;

internal static class Program
{

    private static DisplayBoard Board;

    private static void Main()
    {
        TaskFactory factory = new();
        Task hardwareInitializationTask = factory.StartNew(HardwareInitializer.Setup);
        
        AttackTable.SetUp();
        
        // Run JIT.
        Perft.MoveGeneration(Backend.Board.Default(), 4, false);
        
        Console.Clear();
        DrawCycle.OutputTitle();

        Start:
        string[] args = Console.ReadLine()?.Split(" ");
        
        if (args == null) goto Start;
        
        if (args[0].ToLower().Equals("position")) {
            try {
                Board = args[1].ToLower() switch
                {
                    "startpos" => DisplayBoard.Default(),
                    "fen" => DisplayBoard.FromFen(string.Join(" ", args[2..])),
                    _ => throw new InvalidOperationException("Please enter a valid position.")
                };
            } catch (InvalidOperationException) {}
            goto Start;
        }

        if (args[0].ToLower().Equals("perft")) {
            if (Board == null) {
                Console.WriteLine("Please first define a position.");
                goto Start;
            }
            
            RunPerft(int.Parse(args[1]));
            goto Start;
        }

        if (args[0].ToLower().Equals("d")) {
            hardwareInitializationTask.Wait();
            DrawCycle.Draw(Board);
            goto Start;
        }

        if (args[0].ToLower().Equals("play")) {
            hardwareInitializationTask.Wait();
            OperationCycle.Cycle(Board);
            goto Start;
        }
    }

    [SuppressMessage("ReSharper", "UseStringInterpolation")]
    private static void RunPerft(int depth = 1)
    {
        Console.WriteLine("Running PERFT tests: ");

        Console.WriteLine("Running depth " + depth + ": ");
        
        Stopwatch watch = new();
        watch.Start();
        ulong result = Perft.MoveGeneration(Board, depth);
        watch.Stop();
            
        string output = "Searched " + result.ToString("N0") + " nodes (" + watch.ElapsedMilliseconds + " ms).";
        Console.WriteLine(output);
    }

}