using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Backend;
using Backend.Data.Enum;
using Backend.Data.Move;
using Engine;
using Engine.Struct;

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

        string command = Environment.CommandLine;
        if (command.ToLower().Contains("--uci=true")) {
            goto Start;
        }
        
        Console.Clear();
        DrawCycle.OutputTitle();

        Start:
        string[] args = Console.ReadLine()?.Split(" ");
        
        if (args == null) goto Start;

        if (args[0].ToLower().Equals("uci")) {
            StreamWriter standardOutput = new(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
            UniversalChessInterface.Setup();
            UniversalChessInterface.LaunchUci();
            return;
        }
        
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
        
        if (Board == null) {
            Console.WriteLine("Please first define a position.");
            goto Start;
        }

        if (args[0].ToLower().Equals("perft")) {
            RunPerft(int.Parse(args[1]));
            goto Start;
        }

        if (args[0].ToLower().Equals("d") || args[0].ToLower().Equals("d~")) {
            Console.WriteLine(Board.ToString(args[0].ToLower().Equals("d~")));
            goto Start;
        }

        if (args[0].ToLower().Equals("bestmove")) {
            int depth = 8;
            if (args.Length > 1) depth = int.Parse(args[1]);

            MoveSearch moveSearch = new(Board);
            for (int i = 1; i < depth + 1; i++) {
                Stopwatch sw = new();
                sw.Start();
                SearchedMove bestMove = moveSearch.SearchAndReturn(i);
                sw.Stop();
                
                Console.Write(i + ": " + (bestMove.From.ToString() + bestMove.To).ToLower());
                if (bestMove.Promotion != Promotion.None) {
                    Console.Write("=" + bestMove.Promotion.ToString()[0]);
                }
                
                Console.Write(" [" + bestMove.Score + "]");
                Console.WriteLine(" (" + sw.ElapsedMilliseconds + " ms)");
            }
            
            goto Start;
        }

        if (args[0].ToLower().Equals("moves")) {
            for (int i = 1; i < args.Length; i++) {
                Square from = Enum.Parse<Square>(args[i][..2], true);
                Square to = Enum.Parse<Square>(args[i][2..4], true);
                Promotion promotion = Promotion.None;
                if (args[i].Length > 4 && args[i][4].Equals('=')) {
                    promotion = args[i].ToUpper()[5] switch
                    {
                        'R' => Promotion.Rook,
                        'N' => Promotion.Knight,
                        'B' => Promotion.Bishop,
                        'Q' => Promotion.Queen,
                        _ => Promotion.None
                    };
                }

                Board.SecureMove(from, to, promotion);
            }
            goto Start;
        }

        if (args[0].ToLower().Equals("play")) {
            hardwareInitializationTask.Wait();
            bool againstSn = false;
            PieceColor snColor = PieceColor.Black;
            if (args.Length > 2) {
                againstSn = args[1].ToLower().Equals("sn");
                if (args[2].ToLower().Equals("w")) snColor = PieceColor.White;
            }
            OperationCycle.Cycle(Board, againstSn, snColor);
            goto Start;
        }
    }

    [SuppressMessage("ReSharper", "UseStringInterpolation")]
    private static void RunPerft(int depth = 1)
    {
        Console.WriteLine("Running PERFT @ depth " + depth + ": ");
        
        Stopwatch watch = new();
        watch.Start();
        ulong result = Perft.MoveGeneration(Board, depth);
        watch.Stop();
            
        string output = "Searched " + result.ToString("N0") + " nodes (" + watch.ElapsedMilliseconds + " ms).";
        Console.WriteLine(output);
    }

}