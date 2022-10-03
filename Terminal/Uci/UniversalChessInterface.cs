using System;
using System.Threading;
using System.Threading.Tasks;
using Backend;
using Backend.Data;
using Backend.Data.Enum;
using Backend.Data.Struct;
using Backend.Engine;

namespace Terminal.Uci;

public static class UniversalChessInterface
{

    private const string NAME = "StockNemo";
    private const string AUTHOR = "Shaheryar";

    private static MoveTranspositionTable TranspositionTable;
    private static int TranspositionTableSizeMb = 16;

    private static DisplayBoard Board = DisplayBoard.Default();
    private static bool Busy;

    public static void Setup()
    {
        Busy = false;
        UciStdInputThread.CommandReceived += (_, input) => HandleSetOption(input);
        UciStdInputThread.CommandReceived += (_, input) => HandleIsReady(input);
        UciStdInputThread.CommandReceived += (thread, input) => HandleQuit((Thread)thread, input);
        UciStdInputThread.CommandReceived += (_, input) => HandlePosition(input);
        UciStdInputThread.CommandReceived += (_, input) => HandleDraw(input);
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
        Console.WriteLine("readyok");
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
                Board = DisplayBoard.Default();

                argsParsed++;
                break;
            case "fen":
                string p = args[2];
                string s = args[3];
                string c = args[4];
                string ep = args[5];

                Board = DisplayBoard.FromFen(p + " " + s + " " + c + " " + ep);
                
                argsParsed += 7;
                break;
            default:
                throw new InvalidOperationException("Invalid Position provided.");
        }

        if (args.Length < argsParsed + 1) {
            Busy = false;
            return;
        }
        
        // Once we've loaded the position, we can apply moves.
        if (args[argsParsed].ToLower().Equals("moves")) {
            for (int i = argsParsed + 1; i < args.Length; i++) {
                Square from = Enum.Parse<Square>(args[i][..2], true);
                Square to = Enum.Parse<Square>(args[i][2..4], true);
                Promotion promotion = Promotion.None;
                if (args[i].Length > 4) {
                    promotion = args[i].ToLower()[4] switch
                    {
                        'r' => Promotion.Rook,
                        'n' => Promotion.Knight,
                        'b' => Promotion.Bishop,
                        'q' => Promotion.Queen,
                        _ => Promotion.None
                    };
                }

                Board.GuiMove(from, to, promotion);
            }
        }

        Busy = false;
    }

    private static void HandleDraw(string input)
    {
        switch (input.ToLower()) {
            case "draw":
            case "d":
                if (Board is null) return;
                Console.WriteLine(Board.ToString());
                break;
        }
    }

    private static void HandleGo(string input)
    {
        string[] args = input.Split(" ");
        if (!args[0].ToLower().Equals("go")) return;
        if (Board == null) return;
        
        if (input.ToLower().Contains("perft")) {
            // Just run PERFT.
            Program.RunPerft(Board, int.Parse(args[2]));
            return;
        }

        const int maxTime = 999_999_999;
        const int maxDepth = 63;
        int time = maxTime;
        int depth = maxDepth;
        int movesToGo = -1;

        if (args.Length == 1) {
            TimeManager.Setup();
            goto SkipParameter;
        }

        bool timeSpecified = false;
        Span<int> timeForColor = stackalloc int[2];
        Span<int> timeIncForColor = stackalloc int[2];
        
        int argPosition = 1;
        
        while (argPosition < args.Length) {
            switch (args[argPosition].ToLower()) {
                case "wtime":
                    timeForColor[0] = int.Parse(args[++argPosition]);
                    time = 0;
                    break;
                case "btime":
                    timeForColor[1] = int.Parse(args[++argPosition]);
                    time = 0;
                    break;
                case "winc":
                    timeIncForColor[0] = int.Parse(args[++argPosition]);
                    time = 0;
                    break;
                case "binc":
                    timeIncForColor[1] = int.Parse(args[++argPosition]);
                    time = 0;
                    break;
                case "movestogo":
                    movesToGo = int.Parse(args[++argPosition]);
                    time = 0;
                    break;
                case "depth":
                    depth = Math.Min(maxDepth, int.Parse(args[++argPosition]));
                    break;
                case "movetime":
                    time = int.Parse(args[++argPosition]);
                    timeSpecified = true;
                    break;
            }

            argPosition++;
        }
        
        if (time == maxTime) TimeManager.Setup();
        else if (timeSpecified) TimeManager.Setup(time);
        else TimeManager.Setup(Board, timeForColor, timeIncForColor, movesToGo);

        SkipParameter:
        TaskFactory factory = new();
        OrderedMoveEntry bestMove;

        factory.StartNew(() =>
        {
            // ReSharper disable once AccessToModifiedClosure
            MoveSearch search = new(Board.Clone(), TranspositionTable);
            Busy = true;
            bestMove = search.IterativeDeepening(depth);
            Busy = false;
            string from = bestMove.From.ToString().ToLower();
            string to = bestMove.To.ToString().ToLower();
            string promotion = bestMove.Promotion != Promotion.None ? bestMove.Promotion.ToUciNotation() : "";
            Console.WriteLine("bestmove " + from + to + promotion);
#if DEBUG
            Console.WriteLine("TT Count: " + search.TableCutoffCount);
#endif
        }, TimeManager.ThreadToken);
    }

    private static void HandleStop(string input)
    {
        if (!input.ToLower().Equals("stop")) return;
        if (!Busy) return;
        TimeManager.ChangeTime(0);
    }

}