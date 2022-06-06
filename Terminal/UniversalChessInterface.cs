using System;
using System.Threading;
using System.Threading.Tasks;
using Backend;
using Backend.Data.Enum;
using Engine;
using Engine.Data;
using Engine.Data.Struct;

namespace Terminal;

public static class UniversalChessInterface
{

    private const string NAME = "StockNemo";
    private const string AUTHOR = "Shaheryar";

    private static MoveTranspositionTable TranspositionTable;
    private static int TranspositionTableSizeMb = 16;

    private static Board Board;
    private static bool Busy;
    private static CancellationTokenSource SearchCancellationSource;
    private static int MoveCount;

    public static void Setup()
    {
        Busy = false;
        UciStdInputThread.CommandReceived += (_, input) => HandleSetOption(input);
        UciStdInputThread.CommandReceived += (_, input) => HandleIsReady(input);
        UciStdInputThread.CommandReceived += (thread, input) => HandleQuit((Thread)thread, input);
        UciStdInputThread.CommandReceived += (_, input) => HandlePosition(input);
        UciStdInputThread.CommandReceived += (_, input) => HandleGo(input);
        UciStdInputThread.CommandReceived += (_, input) => HandleStop(input);
    }

    public static void LaunchUci()
    {
        // Initialize default UCI parameters.
        TranspositionTable = MoveTranspositionTable.GenerateTable(TranspositionTableSizeMb);
        
        // Provide identification information.
        Console.WriteLine("id name " + NAME);
        Console.WriteLine("id author " + AUTHOR);
        Console.WriteLine("option name Hash type spin default 16 min 4 max 512");
        
        // Let GUI know engine is ready in UCI mode.
        Console.WriteLine("uciok");
        
        // Start an input thread.
        Thread inputThread = new(UciStdInputThread.StartAcceptingInput);
        inputThread.Start();
    }

    private static void HandleSetOption(string input)
    {
        if (!input.ToLower().Contains("setoption")) return;

        string[] args = input.Split(" ");
        switch (args[2]) {
            case "Hash":
                TranspositionTableSizeMb = int.Parse(args[4]);
                Busy = true;
                TranspositionTable.FreeMemory();
                TranspositionTable = null;
                TranspositionTable = MoveTranspositionTable.GenerateTable(TranspositionTableSizeMb);
                Busy = false;
                break;
        }
    }

    private static void HandleIsReady(string input)
    {
        if (!input.ToLower().Equals("isready")) return;
        Task.Run(() =>
        {
            while (Busy) {}
            Console.WriteLine("readyok");
        });
    }

    private static void HandleQuit(Thread thread, string input)
    {
        if (!input.ToLower().Equals("quit")) return;
        TranspositionTable.FreeMemory();
        TranspositionTable = null;
        UciStdInputThread.Running = false;
        thread.IsBackground = true;
        Environment.Exit(0);
    }

    private static void HandlePosition(string input)
    {
        string[] args = input.Split(" ");
        if (!args[0].ToLower().Equals("position")) return;
        if (args.Length == 1) return;
        Busy = true;
        int argsParsed = 1;
        switch (args[1].ToLower()) {
            case "startpos":
                Board = Board.Default();

                argsParsed++;
                break;
            case "fen":
                string p = args[2];
                string s = args[3];
                string c = args[4];
                string ep = args[5];

                Board = Board.FromFen(p + " " + s + " " + c + " " + ep);
                
                argsParsed += 7;
                break;
            default:
                throw new InvalidOperationException("Invalid Position provided.");
        }

        if (args.Length < argsParsed) {
            Busy = false;
            return;
        }
        
        // Once we've loaded the position, we can apply moves.
        if (args[argsParsed].ToLower().Equals("moves")) {
            MoveCount = args.Length - (argsParsed + 1);
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

        Busy = false;
    }

    private static void HandleGo(string input)
    {
        string[] args = input.Split(" ");
        if (!args[0].ToLower().Equals("go")) return;
        if (args.Length == 1) return;

        TaskFactory factory = new();
        SearchedMove bestMove = new(Square.Na, Square.Na, Promotion.None, 0);

        int time = 3500;
        
        Span<int> timeForColor = stackalloc int[2];
        Span<int> timeIncForColor = stackalloc int[2];
        PieceColor color = Board.WhiteTurn ? PieceColor.White : PieceColor.Black;
        if (input.ToLower().Contains("wtime") || input.ToLower().Contains("btime")) {
            timeForColor[0] = int.Parse(args[2]);
            timeForColor[1] = int.Parse(args[4]);
            timeIncForColor[0] = int.Parse(args[6]);
            timeIncForColor[1] = int.Parse(args[8]);

            time = timeForColor[(int)color] / 20;

            if (MoveCount > 10) {
                time += timeIncForColor[(int)color] / 2;
            }

            if (MoveCount > 20) {
                int dTime = timeForColor[(int)color] - timeForColor[(int)Util.OppositeColor(color)];
                if (dTime > 3000) time += dTime / 3;
            }
        } else if (input.ToLower().Contains("movetime")) {
            time = int.Parse(args[2]);
        }

        SearchCancellationSource = new CancellationTokenSource();
        SearchCancellationSource.CancelAfter(time);
        factory.StartNew(() =>
        {
            // ReSharper disable once AccessToModifiedClosure
            MoveSearch search = new(Board.Clone(), TranspositionTable, SearchCancellationSource.Token);
            int depth = 1;
            Busy = true;
            try {
                while (!SearchCancellationSource.Token.IsCancellationRequested) {
                    bestMove = search.SearchAndReturn(depth);
                            
                    Console.Write(
                        "info depth " + depth + " nodes " + search.TotalNodeSearchCount + " pv " + 
                        bestMove.From.ToString().ToLower() + bestMove.To.ToString().ToLower()
                    );
                    if (bestMove.Promotion != Promotion.None)
                        Console.Write(bestMove.Promotion.ToString().ToLower()[0]);

                    Console.WriteLine();
                    depth++;
                }
            } catch (OperationCanceledException) {} 
            Busy = false;
            string from = bestMove.From.ToString().ToLower();
            string to = bestMove.To.ToString().ToLower();
            string promotion = bestMove.Promotion != Promotion.None ? 
                bestMove.Promotion.ToString()[0].ToString().ToLower() : "";
            Console.WriteLine("bestmove " + from + to + promotion);
            Console.WriteLine("TT Count: " + search.TableCutoffCount);
            MoveCount++;
        }, SearchCancellationSource.Token);
    }

    private static void HandleStop(string input)
    {
        if (!input.ToLower().Equals("stop")) return;
        if (!Busy) return;
        SearchCancellationSource.Cancel();
    }

}