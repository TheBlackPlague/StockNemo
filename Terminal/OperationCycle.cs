using System;
using Backend.Data.Enum;
using Engine;
using Engine.Data;
using Engine.Data.Struct;

namespace Terminal;

internal static class OperationCycle
{

    private static bool Exit;

    private static MoveTranspositionTable Table;

    public static void Cycle(DisplayBoard board, bool againstSn = false, PieceColor snColor = PieceColor.None)
    {
        if (againstSn) Table = MoveTranspositionTable.GenerateTable(32);
        while (!Exit) {
            Start:
            DrawCycle.Draw(board);
            
            Square from = Square.Na;
            Square to = Square.Na;
            Promotion promotion = Promotion.None;
            
            FromSelection:
            string colorToMove = board.WhiteTurn ? "White" : "Black";

            if (againstSn && Enum.Parse<PieceColor>(colorToMove, true) == snColor) {
                MoveSearch moveSearch = new(board, Table);
                Console.WriteLine("Searching best move...");
                SearchedMove bestMove = moveSearch.SearchAndReturn(8);
                from = bestMove.From;
                to = bestMove.To;
                promotion = bestMove.Promotion;
                goto Move;
            }
            
            Console.Write(colorToMove + " to move (enter square to highlight moves): ");
            string fromSelection = Console.ReadLine()?.ToUpper();

            if (fromSelection!.Length == 2) {
                from = Enum.Parse<Square>(fromSelection);
                if (!VerifyFromIsCorrectTurn(board, from)) goto FromSelection;
            } else if (fromSelection.Length == 4) {
                from = Enum.Parse<Square>(fromSelection[..2]);
                to = Enum.Parse<Square>(fromSelection[2..]);
                if (!VerifyFromIsCorrectTurn(board, from)) {
                    to = Square.Na;
                    goto FromSelection;
                }
            }
            
            if (board.EmptyAt(from)) {
                Console.WriteLine("That square has no piece.");
                to = Square.Na;
                goto FromSelection;
            }
            
            if (to != Square.Na) goto FromAndToSelected;

            ToSelection:
            board.HighlightMoves(from);
            DrawCycle.Draw(board);
            Console.Write("Please enter a move from the highlighted moves: ");
            string toSelection = Console.ReadLine()?.ToUpper();

            if (toSelection!.Equals("Z")) goto Start;

            if (toSelection.Length != 2) goto ToSelection;
            to = Enum.Parse<Square>(toSelection);
            
            FromAndToSelected:
            (Piece piece, PieceColor color) = board.At(from);
            if (piece == Piece.Pawn) {
                switch (color) {
                    case PieceColor.White when from is > Square.H6 and < Square.A8:
                    case PieceColor.Black when from is > Square.H1 and < Square.A3:
                        goto PromotionSelection;
                    case PieceColor.None:
                    default:
                        goto Move;
                }

                PromotionSelection:
                Console.Write("Enter promotion out of [R, N, B, Q]: ");
                
                string promotionInput = Console.ReadLine()?.ToUpper();
                if (promotionInput!.Equals("Z")) goto Start;

                try {
                    promotion = promotionInput switch
                    {
                        "R" => Promotion.Rook,
                        "N" => Promotion.Knight,
                        "B" => Promotion.Bishop,
                        "Q" => Promotion.Queen,
                        _ => throw new InvalidOperationException()
                    };
                } catch (InvalidOperationException) {
                    Console.WriteLine("Invalid promotion entered. Enter 'Z' if you want to unselect move.");
                    goto PromotionSelection;
                }
            }
            
            Move:
            MoveResult result = board.SecureMove(from, to, promotion);
            switch (result) {
                case MoveResult.Fail:
                    Console.WriteLine("Illegal move selected.");
                    goto Start;
                case MoveResult.Success:
                    break;
                case MoveResult.SuccessAndCheck:
                    DrawCycle.Draw(board);
                    string underCheck = board.WhiteTurn ? "White" : "Black";
                    Console.WriteLine(underCheck + " is under check!");
                    goto FromSelection;
                case MoveResult.Checkmate:
                    DrawCycle.Draw(board);
                    string winner = board.WhiteTurn ? "Black" : "White";
                    Console.WriteLine("CHECKMATE. " + winner + " won!");
                    Exit = true;
                    break;
                default:
                    throw new InvalidOperationException("Invalid MoveResult provided.");
            }
        }
    }

    private static bool VerifyFromIsCorrectTurn(DisplayBoard board, Square from)
    {
        if (board.WhiteTurn && board.At(from).Item2 == PieceColor.Black) {
            Console.WriteLine("It's White Turn.");
            return false;
        }
        // ReSharper disable once InvertIf
        if (!board.WhiteTurn && board.At(from).Item2 == PieceColor.White) {
            Console.WriteLine("It's Black Turn.");
            return false;
        }

        return true;
    }

}