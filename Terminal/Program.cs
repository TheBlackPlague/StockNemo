using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Backend;
using Backend.Board;
using Test;
using Version = Backend.Version;

namespace Terminal
{

    internal static class Program
    {

        private static BitDataBoard Board;

        private static void Main(string[] args)
        {
            switch (args.Length) {
                case > 0 when args[0] == "perft":
                    RunPerft();
                    return;
                case > 0 when args[0] == "fen":
                    Board = BitDataBoard.FromFen(args[1]);
                    break;
                default:
                    Board = BitDataBoard.Default();
                    break;
            }

            OperationLoop();
        }

        private static void OperationLoop()
        {
            while (true) {
                Draw();
                
                (int, int) from;
                (int, int) to;
                
                FromSelection:
                Console.Write("Select piece to move (or enter full move in AN): ");
                string toMove = Console.ReadLine()?.ToUpper();
                
                if (toMove?.Length != 2) {
                    switch (toMove?.Length) {
                        case 3:
                        {
                            if (toMove.Equals("ALL")) {
                                PieceColor color = Board.IsWhiteTurn() ? PieceColor.White : PieceColor.Black;
                                Board.HighlightMoves(color);
                                Draw();
                                Console.WriteLine("Highlighting moves for: " + color + "\n");
                                goto FromSelection;
                            }

                            break;
                        }
                        case 4:
                        {
                            from = Util.ChessStringToTuple(toMove[..2]);
                            to = Util.ChessStringToTuple(toMove[2..]);
                        
                            if (!VerifyTurn(from)) goto FromSelection;
                            goto Move;
                        }
                    }

                    Console.WriteLine("Please enter the file and rank.");
                    goto FromSelection;
                }

                from = Util.ChessStringToTuple(toMove);
                
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

                to = Util.ChessStringToTuple(moveTo);
                
                Move:
                MoveAttempt result = Board.SecureMove(from, to);
                if (result == MoveAttempt.Fail) {
                    Console.WriteLine("Illegal move selected.");
                    goto ToSelection;
                }

                if (result == MoveAttempt.Checkmate) {
                    Draw();
                    string winner = Board.IsWhiteTurn() ? "Black" : "White";
                    Console.WriteLine("CHECKMATE. " + winner + " won!");
                    break;
                }

                if (result == MoveAttempt.SuccessAndCheck) {
                    Draw();
                    string underCheck = Board.IsWhiteTurn() ? "White" : "Black";
                    Console.WriteLine(underCheck + " is under check!");
                    goto FromSelection;
                }

                // Sleep for a bit to show work being done
                // It's funny because the program is lightning fast
                Thread.Sleep(200);
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
            Console.WriteLine(Board.ToString());
        }

        private static bool VerifyTurn((int, int) from)
        {
            if (Board.IsWhiteTurn() && Board.At(from).Item2 == PieceColor.Black) {
                Console.WriteLine("It's White Turn.");
                return false;
            }
            // ReSharper disable once InvertIf
            if (!Board.IsWhiteTurn() && Board.At(from).Item2 == PieceColor.White) {
                Console.WriteLine("It's Black Turn.");
                return false;
            }

            return true;
        }

        [SuppressMessage("ReSharper", "UseStringInterpolation")]
        private static void RunPerft()
        {
            Console.WriteLine("Running PERFT tests: ");

            MoveDepthTest test = new();
            
            Console.WriteLine("Running Depth 0: ");
            Stopwatch watch = new();
            watch.Start();
            (ulong, ulong) result = test.Depth0();
            watch.Stop();
            
            string output = "Searched " + result.Item2.ToString("N0") + " nodes (" + 
                            watch.ElapsedMilliseconds + " ms).";
            Console.WriteLine(output);
            
            Console.WriteLine("Running Depth 1: ");
            watch = new Stopwatch();
            watch.Start();
            result = test.Depth1();
            watch.Stop();
            
            output = "Searched " + result.Item2.ToString("N0") + " nodes (" + watch.ElapsedMilliseconds + " ms).";
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
        }

    }

}