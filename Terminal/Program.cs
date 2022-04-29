using System;
using System.Threading;
using Backend;
using Backend.Board;
using Version = Backend.Version;

namespace Terminal
{

    internal static class Program
    {

        private static readonly DataBoard Board = new();

        private static void Main()
        {
            OperationLoop();
        }

        private static void OperationLoop()
        {
            while (true) {
                Draw();
                
                FromSelection:
                Console.Write("Select piece to move: ");
                string toMove = Console.ReadLine()?.ToUpper();

                if (toMove?.Length != 2) {
                    Console.WriteLine("Please enter the file and rank.");
                    goto FromSelection;
                }

                (int, int) from = Util.ChessStringToTuple(toMove);
                
                if (Board.EmptyAt(from)) {
                    Console.WriteLine("Cannot be moving empty space.");
                    goto FromSelection;
                }

                if (Board.MoveCount() % 2 == 0 && Board.At(from).Item2 == PieceColor.Black) {
                    Console.WriteLine("It's White Turn.");
                    goto FromSelection;
                }
                if (Board.MoveCount() % 2 == 1 && Board.At(from).Item2 == PieceColor.White) {
                    Console.WriteLine("It's Black Turn.");
                    goto FromSelection;
                }
                
                Board.HighlightMoves(from);
                Draw();
                
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

                (int, int) to = Util.ChessStringToTuple(moveTo);
                
                MoveAttempt result = Board.SecureMove(from, to);
                if (result == MoveAttempt.Fail) {
                    Console.WriteLine("Illegal move selected.");
                    goto ToSelection;
                }

                if (result == MoveAttempt.Checkmate) {
                    Draw();
                    string winner = Board.MoveCount() % 2 == 0 ? "Black" : "White";
                    Console.WriteLine("CHECKMATE. " + winner + " won!");
                    break;
                }

                if (result == MoveAttempt.SuccessAndCheck) {
                    Draw();
                    string underCheck = Board.MoveCount() % 2 == 0 ? "White" : "Black";
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
            Console.WriteLine("ChessEmulator v" + Version.Get());
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

    }

}