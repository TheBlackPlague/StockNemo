using System;
using Backend;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Terminal.Interactive;

internal static class InteractiveInterface
{
    
    private static readonly Menu StartMenu = new(
        "Welcome to StockNemo Interactive CLI. Please select an option: ", 
        DrawCycle.OutputTitle,
        new Option("Load Default Position", () => Board = DisplayBoard.Default()),
        new Option("Load from FEN", InputFen)
    );

    private static bool Restart;

    private static bool Check;
    private static bool CheckMate;

    private static DisplayBoard Board = DisplayBoard.Default();

    public static void Start()
    {
        while (true) {
            Console.Clear();
            DrawCycle.OutputTitle();
            Console.WriteLine("");
            StartMenu.Display();
            StartMenu.ListenForCursorUpdate();
            if (Restart) {
                Restart = false;
                continue;
            }
            break;
        }
        
        MainLoop();
        
        Environment.Exit(0);
    }

    private static void InputFen()
    {
        Console.Clear();
        DrawCycle.OutputTitle();
        Console.WriteLine("");
        Console.Write("Please enter FEN (write \"ESC\" to go back): ");
        string fen = Console.ReadLine();
        
        if (fen!.ToLower().Equals("esc")) {
            Restart = true;
        }
        
        Board = DisplayBoard.FromFen(fen);
    }

    private static void MainLoop()
    {
        while (true) {
            DrawBoard();
            if (CheckMate) break;

            Square from, to;
            Promotion promotion = Promotion.None;
            
            Console.WriteLine(Board.ColorToMove + " to move.");
            Console.Write("Enter the full move (ex. a2a4, a7a8) without promotion or " +
                          "square (ex. a2) in UCI Notation: ");

            string input = Console.ReadLine();
            
            if (input!.Length == 2) {
                // If input is length of 2, it's a square entered.
                from = Enum.Parse<Square>(input, true);
                
                // Verify the square.
                if (!VerifyFromSquare(from)) continue;
                
                // We should highlight the moves to make move selection easy.
                Board.HighlightMoves(from);
                DrawBoard();
            } else if (input.Length == 4) {
                // If input is length of 4, it's a move entered.
                
                // Parse and verify square.
                from = Enum.Parse<Square>(input[..2], true);
                if (!VerifyFromSquare(from)) continue;

                to = Enum.Parse<Square>(input[2..], true);
                if (!VerifyToSquare(from, to)) goto EnterTo;
                goto SelectPromotion;
            } else {
                Console.WriteLine("Invalid Input. Press any key to retry.");
                Console.ReadKey();
                continue;
            }

            EnterTo:
            Console.WriteLine("Enter the square (ex. a2) you want to move this piece to in UCI notation " +
                              "(ESC to go back): ");
            input = Console.ReadLine();
            if (input!.Length == 2) {
                // User entered a two square.
                to = Enum.Parse<Square>(input, true);

                if (!VerifyToSquare(from, to)) goto EnterTo;
            } else {
                Console.WriteLine("Invalid Input. Press any key to retry.");
                Console.ReadKey();
                goto EnterTo;
            }
            
            SelectPromotion:
            if (MoveList.WithoutProvidedPins(Board, from).Promotion) {
                // A promotion is possible.
                Menu promotionMenu = new(
                    "Enter the promotion: ",
                    DrawBoard,
                    new Option("Rook", () => promotion = Promotion.Rook),
                    new Option("Knight", () => promotion = Promotion.Knight),
                    new Option("Bishop", () => promotion = Promotion.Bishop),
                    new Option("Queen", () => promotion = Promotion.Queen),
                    new Option("Back", () => promotion = Promotion.None)
                );
                
                DrawBoard();
                promotionMenu.Display();
                promotionMenu.ListenForCursorUpdate();
                
                if (promotion == Promotion.None) continue;
            }


            Check = false;
            CheckMate = false;
            Board.GuiMove(from, to, promotion);

            // If opponent has no moves to make after our move, it's checkmate.
            MoveList opposingMoveList = new(Board, Board.ColorToMove);
            if (opposingMoveList.Count == 0) CheckMate = true;

            // If we're attacking opponent's king, then the opponent is under check.
            PieceColor attackingColor = Util.OppositeColor(Board.ColorToMove);
            if (MoveList.UnderAttack(Board, Board.KingLoc(Board.ColorToMove), attackingColor)) Check = true;
        }
    }

    private static bool VerifyFromSquare(Square from)
    {
        if (from == Square.Na) {
            Console.WriteLine("Invalid [FROM] square provided. Press any key to retry.");
            Console.ReadKey();
            return false;
        }

        // We should verify it's the square for correct color.
        // ReSharper disable once InvertIf
        if (!Board.All(Board.ColorToMove)[from]) {
            Console.WriteLine("That square doesn't belong to " + Board.ColorToMove + ". \n" +
                              "Press any key to retry.");
            Console.ReadKey();
            return false;
        }

        return true;
    }

    private static bool VerifyToSquare(Square from, Square to)
    {
        if (to == Square.Na) {
            Console.WriteLine("Invalid [TO] square provided. Press any key to retry.");
            Console.ReadKey();
            
            Board.HighlightMoves(from);
            DrawBoard();
            return false;
        }
        
        // We should verify if the move is legal.
        MoveList moveList = MoveList.WithoutProvidedPins(Board, from);
        // ReSharper disable once InvertIf
        if (!moveList.Moves[to]) {
            Console.WriteLine("Illegal move provided. Press any key to retry.");
            Console.ReadKey();
                    
            Board.HighlightMoves(from);
            DrawBoard();
            return false;
        }

        return true;
    }

    private static void DrawBoard()
    {
        DrawCycle.Draw(Board);
        if (Check) Console.WriteLine(Board.ColorToMove + " is under check.");
        if (CheckMate) Console.WriteLine("Checkmate - " + Util.OppositeColor(Board.ColorToMove) + " won!");
    }

}