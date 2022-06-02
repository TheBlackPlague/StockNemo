using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend;
using Backend.Data.Enum;
using Engine;
using Engine.Struct;

namespace Terminal;

public static class UniversalChessInterface
{

    private const string NAME = "StockNemo";
    private const string AUTHOR = "Shaheryar";

    private static Board Board;

    public static void LaunchInUciToGuiMode()
    {
        // Provide identification information.
        Console.WriteLine("id name " + NAME);
        Console.WriteLine("id author " + AUTHOR);
        
        // Let GUI know engine is ready in UCI mode.
        Console.WriteLine("uciok");
        
        // Wait for GUI to ask if we're done setting up.
        string[] args = WaitForCommands(new []{ "isready", "quit" }).Split(" ");
        if (args[0].ToLower() == "quit") return;

        TaskFactory factory = new();
        // Let GUI know we're done setting up.
        Console.WriteLine("readyok");
        
        ParseCommand:
        args = WaitForCommands(new [] { "ucinewgame", "position", "go", "quit" }).Split(" ");
        
        switch (args[0].ToLower()) {
            case "quit":
                return;
            // Not all GUIs will support this... we should just wait for the position.
            case "ucinewgame":
                goto ParseCommand;
            // Load the position.
            case "position":
                // If only the command was provided, it was wrong and we should wait for next command to be
                // correct.
                if (args.Length == 1) goto ParseCommand;

                int argsParsed = 1;
                switch (args[1].ToLower()) {
                    // In the case of startpos, it's the default position.
                    case "startpos":
                        Board = Board.Default();
                        break;
                    // In the case of fen, we can extract the data from the args and load it into the board.
                    case "fen":
                    {
                        string p = args[2];
                        string s = args[3];
                        string c = args[4];
                        string ep = args[5];

                        Board = Board.FromFen(p + " " + s + " " + c + " " + ep);
                
                        argsParsed += 6;
                        break;
                    }
                    default:
                        throw new InvalidOperationException("No proper position argument provided.");
                }

                // Once we've loaded the position, we can apply moves.
                if (args[argsParsed].ToLower().Equals("moves")) {
                    for (int i = argsParsed + 1; i < args.Length; i++) {
                        Square from = Enum.Parse<Square>(args[i][..2], true);
                        Square to = Enum.Parse<Square>(args[i][2..4], true);
                        Promotion promotion = Promotion.None;
                        if (args[i].Length > 4) {
                            promotion = args[i][4] switch
                            {
                                'r' => Promotion.Rook,
                                'n' => Promotion.Knight,
                                'b' => Promotion.Bishop,
                                'q' => Promotion.Queen,
                                _ => Promotion.None
                            };
                        }

                        Board.SecureMove(from, to, promotion);
                    }
                }
                break;
            case "go":
                goto GoCommandRun;
            default:
                goto ParseCommand;
        }

        GoCommandRun:
        switch (args[0].ToLower()) {
            case "quit":
                return;
            case "go":
                SearchedMove bestMove = new(Square.Na, Square.Na, Promotion.None, 0);
                
                switch (args[1].ToLower()) {
                    case "movetime":
                        int time = int.Parse(args[2]);

                        CancellationTokenSource source = new();
                        source.CancelAfter(time);
                        
                        Task searchTask = factory.StartNew(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MoveSearch search = new(Board.Clone(), source.Token);
                            int depth = 1;
                            try {
                                while (true) {
                                    bestMove = search.SearchAndReturn(depth);
                                    Console.Write("info depth " + depth + " " + bestMove.From + bestMove.To);
                                    if (bestMove.Promotion != Promotion.None)
                                        Console.Write(bestMove.Promotion.ToString()[0]);
                                
                                    Console.WriteLine();
                                    depth++;
                                }
                            } catch (OperationCanceledException) {}
                        }, source.Token);

                        CancellationTokenSource inputCancellationSource = new();
                        // ReSharper disable once MethodSupportsCancellation
                        Task<string> input = Task.Run(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            Task<string> innerTask = Task.Run(Console.ReadLine, inputCancellationSource.Token);
                            // ReSharper disable once AccessToModifiedClosure
                            innerTask.Wait(inputCancellationSource.Token);
                            return innerTask.Result;
                        }, inputCancellationSource.Token);

                        while (!input.IsCompleted) {
                            if (source.Token.IsCancellationRequested) {
                                goto WithoutInput;
                            }
                        }

                        switch (input.Result!.ToLower()) {
                            case "quit":
                                source.Cancel();
                                return;
                            case "stop":
                                source.Cancel();
                                break;
                            case "isready":
                                // ReSharper disable once MethodSupportsCancellation
                                searchTask.Wait();
                                Console.WriteLine("readyok");
                                break;
                        }

                        WithoutInput:
                        // ReSharper disable once MethodSupportsCancellation
                        searchTask.Wait();
                        
                        break;
                }
                
                string from = bestMove.From.ToString().ToLower();
                string to = bestMove.To.ToString().ToLower();
                string promotion = 
                    bestMove.Promotion != Promotion.None ? bestMove.Promotion.ToString()[0].ToString().ToLower() : "";
                Console.WriteLine("bestmove " + from + to + promotion);
                break;
            default:
                goto ParseCommand;
        }
        
        goto ParseCommand;
    }

    private static string WaitForCommands(string[] commands)
    {
        string[] input = Console.ReadLine()?.Split(" ");
        while (!input!.Any(i => commands.Contains(i.ToLower()))) {
            input = Console.ReadLine()?.Split(" ");
        }

        return string.Join(" ", input);
    }
    
}