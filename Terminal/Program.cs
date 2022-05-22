using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Backend;
using Backend.Data.Enum;
using Backend.Data.Struct;
using Version = Backend.Version;

namespace Terminal
{

    internal static class Program
    {

        private static Board Board;

        private static void Main(string[] args)
        {
            OutputTitle();
            MoveList.SetUp();
            switch (args.Length) {
                case > 0 when args[0] == "perft":
                    RunPerft();
                    return;
                case > 0 when args[0] == "fen":
                    Board = Board.FromFen(args[1]);
                    break;
                default:
                    Board = Board.Default();
                    break;
            }

            OperationLoop();
        }

        private static void OperationLoop()
        {
            while (true) {
                Draw();
                
                Square from;
                Square to;
                
                FromSelection:
                Console.Write("Select piece to move (or enter full move in AN): ");
                string toMove = Console.ReadLine()?.ToUpper();
                
                if (toMove?.Length != 2) {
                    switch (toMove?.Length) {
                        case 3:
                        {
                            if (toMove.Equals("ALL")) {
                                PieceColor color = Board.WhiteTurn ? PieceColor.White : PieceColor.Black;
                                Board.HighlightMoves(color);
                                Draw();
                                Console.WriteLine("Highlighting moves for: " + color + "\n");
                                goto FromSelection;
                            }

                            break;
                        }
                        case 4:
                        {
                            from = Enum.Parse<Square>(toMove[..2]);
                            to = Enum.Parse<Square>(toMove[2..]);
                        
                            if (!VerifyTurn(from)) goto FromSelection;
                            goto Move;
                        }
                    }

                    Console.WriteLine("Please enter the file and rank.");
                    goto FromSelection;
                }

                from = Enum.Parse<Square>(toMove);
                
                if (Board.EmptyAt(from)) {
                    Console.WriteLine("Cannot be moving empty space.");
                    goto FromSelection;
                }

                if (!VerifyTurn(from)) goto FromSelection;

                Board.HighlightMoves(from);
                Draw();
                Console.WriteLine("Highlighting moves for: " + toMove.ToLower() + "\n");
                
                ToSelection:
                Console.Write("Choose a move from the highlighted legal moves: ");
                string moveTo = Console.ReadLine()?.ToUpper();

                if (moveTo == "Z") {
                    Draw();
                    goto FromSelection;
                }

                if (moveTo?.Length != 2) {
                    Console.WriteLine("Please enter the file and rank.");
                    goto ToSelection;
                }

                to = Enum.Parse<Square>(moveTo);
                
                Move:
                MoveAttempt result = Board.SecureMove(from, to);
                if (result == MoveAttempt.Fail) {
                    Console.WriteLine("Illegal move selected.");
                    goto ToSelection;
                }

                if (result == MoveAttempt.Success) {
                    
                }

                if (result == MoveAttempt.Checkmate) {
                    Draw();
                    string winner = Board.WhiteTurn ? "Black" : "White";
                    Console.WriteLine("CHECKMATE. " + winner + " won!");
                    break;
                }

                if (result == MoveAttempt.SuccessAndCheck) {
                    Draw();
                    string underCheck = Board.WhiteTurn ? "White" : "Black";
                    Console.WriteLine(underCheck + " is under check!");
                    goto FromSelection;
                }

                // Sleep for a bit to show work being done
                // It's funny because the program is lightning fast
                // Thread.Sleep(200);
            }
        }

        private static void OutputTitle()
        {
            Console.WriteLine("StockNemo v" + Version.Get());
            Console.WriteLine("Copyright (c) Shaheryar Sohail. All rights reserved.");
            Console.WriteLine("┌───────────────────┐");
            Console.WriteLine("│  Chess Board CLI  │");
            Console.WriteLine("└───────────────────┘");
        }

        private static void Draw()
        {
            Console.Clear();
            OutputTitle();
            string board = Board.ToString();
            Console.WriteLine(board);
        }

        private static bool VerifyTurn(Square from)
        {
            if (Board.WhiteTurn && Board.At(from).Item2 == PieceColor.Black) {
                Console.WriteLine("It's White Turn.");
                return false;
            }
            // ReSharper disable once InvertIf
            if (!Board.WhiteTurn && Board.At(from).Item2 == PieceColor.White) {
                Console.WriteLine("It's Black Turn.");
                return false;
            }

            return true;
        }

        [SuppressMessage("ReSharper", "UseStringInterpolation")]
        private static void RunPerft()
        {
            Console.WriteLine("Running PERFT tests: ");

            Perft test = new();

            Console.WriteLine("Running Depth 1: ");
            Stopwatch watch = new();
            watch.Start();
            (ulong, ulong) result = test.Depth1();
            watch.Stop();
            
            string output = "Searched " + result.Item2.ToString("N0") + " nodes (" + watch.ElapsedMilliseconds + " ms).";
            Console.WriteLine(output);
            
            Console.WriteLine("Running Depth 2: ");
            watch = new Stopwatch();
            watch.Start();
            result = test.Depth2();
            watch.Stop();
            
            output = "Searched " + result.Item2.ToString("N0") + " nodes (" + watch.ElapsedMilliseconds + " ms).";
            Console.WriteLine(output);

            Console.WriteLine("Running Depth 3: ");
            watch = new Stopwatch();
            watch.Start();
            result = test.Depth3();
            watch.Stop();
            
            output = "Searched " + result.Item2.ToString("N0") + " nodes (" + watch.ElapsedMilliseconds + " ms).";
            Console.WriteLine(output);
            
            Console.WriteLine("Running Depth 4: ");
            watch = new Stopwatch();
            watch.Start();
            result = test.Depth4();
            watch.Stop();
            
            output = "Searched " + result.Item2.ToString("N0") + " nodes (" + watch.ElapsedMilliseconds + " ms).";
            Console.WriteLine(output);
            
            Console.WriteLine("Running Depth 5: ");
            watch = new Stopwatch();
            watch.Start();
            result = test.Depth5();
            watch.Stop();
            
            output = "Searched " + result.Item2.ToString("N0") + " nodes (" + watch.ElapsedMilliseconds + " ms).";
            Console.WriteLine(output);
            
            Console.WriteLine("Running Depth 6: ");
            watch = new Stopwatch();
            watch.Start();
            result = test.Depth6();
            watch.Stop();
            
            output = "Searched " + result.Item2.ToString("N0") + " nodes (" + watch.ElapsedMilliseconds + " ms).";
            Console.WriteLine(output);
            
            Console.WriteLine("Running Depth 7: ");
            watch = new Stopwatch();
            watch.Start();
            result = test.Depth7();
            watch.Stop();
            
            output = "Searched " + result.Item2.ToString("N0") + " nodes (" + watch.ElapsedMilliseconds + " ms).";
            Console.WriteLine(output);
        }

    }

}