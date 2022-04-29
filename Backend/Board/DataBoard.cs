using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Backend.Exception;
using Backend.Move;
using BetterConsoles.Core;
using BetterConsoles.Tables;
using BetterConsoles.Tables.Builders;
using BetterConsoles.Tables.Configuration;
using BetterConsoles.Tables.Models;

namespace Backend.Board
{

    public class DataBoard
    {

        public const int UBOUND = 8; // Board Upper Bound
        public const int LBOUND = -1; // Board Lower Bound

        public const int MOVE_SUCCESS = 0;
        public const int MOVE_FAIL = 1;
        public const int MOVE_CHECKMATE = 2;
        
        private readonly (Piece, PieceColor, MovedState)[,] Map = new (Piece, PieceColor, MovedState)[UBOUND, UBOUND];
        
        private readonly Log Log = new();

        private (int, int) WhiteKing;
        private (int, int) BlackKing;

        private (int, int)[] HighlightedMoves = Array.Empty<(int, int)>();

        public DataBoard()
        {
            for (int h = 0; h < UBOUND; h++)
            for (int v = 0; v < UBOUND; v++) {
                Piece piece = v switch
                {
                    1 or 6 => Piece.Pawn,
                    0 or 7 when h is 0 or 7 => Piece.Rook,
                    0 or 7 when h is 1 or 6 => Piece.Knight,
                    0 or 7 when h is 2 or 5 => Piece.Bishop,
                    0 or 7 when h is 3 => Piece.Queen,
                    0 or 7 when h is 4 => Piece.King,
                    _ => Piece.Empty
                };
                PieceColor color = v switch
                {
                    <= 1 => PieceColor.White,
                    >= 6 => PieceColor.Black,
                    _ => PieceColor.None
                };

                Map[h, v] = (piece, color, MovedState.Unmoved);

                if (piece != Piece.King) continue;
                
                // Save king positions for fast fetching
                if (color == PieceColor.White) WhiteKing = (h, v);
                else BlackKing = (h, v);
            }
        }

        public DataBoard Clone(bool parallel = true)
        {
            DataBoard board = new();
            if (parallel) Parallel.For(0, UBOUND, h => 
                Parallel.For(0, UBOUND, v => board.Map[h, v] = Map[h, v]));
            else for (int h = 0; h < UBOUND; h++) for (int v = 0; v < UBOUND; v++) board.Map[h, v] = Map[h, v];

            return board;
        }

        public int MoveCount()
        {
            return Log.Count();
        }

        public (Piece, PieceColor, MovedState) At((int, int) loc)
        {
            return Map[loc.Item1, loc.Item2];
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Global
        public (int, int)[] All(PieceColor color)
        {
            (int, int)[] all = new (int, int)[16];
            int i = 0;
            for (int h = 0; h < UBOUND; h++) for (int v = 0; v < UBOUND; v++)
                if (Map[h, v].Item2 == color)
                    all[i++] = (h, v);

            return all;
        }

        public (int, int) KingLoc(PieceColor color)
        {
            return color == PieceColor.White ? WhiteKing : BlackKing;
        }

        public bool EmptyAt((int, int) loc)
        {
            return Map[loc.Item1, loc.Item2].Item1 == Piece.Empty;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public MoveAttempt SecureMove((int, int) from, (int, int) to)
        {
            LegalMoveSet moveSet = new(this, from);
            (int, int)[] moves = moveSet.Get();

            if (!moves.Contains(to)) return MoveAttempt.Fail;

            Move(from, to);

            PieceColor color = Map[to.Item1, to.Item2].Item2;
            PieceColor oppositeColor = Util.OppositeColor(color);

            LegalMoveSet opposingMoves = new(this, oppositeColor);
            if (opposingMoves.Count() == 0) return MoveAttempt.Checkmate;

            (int, int) kingLoc = KingLoc(oppositeColor);
            return BitBoard(color)[kingLoc.Item1, kingLoc.Item2] ? MoveAttempt.SuccessAndCheck : MoveAttempt.Success;
        }

        internal void Move((int, int) from, (int, int) to)
        {
            (int hF, int vF) = from;
            (int hT, int vT) = to;

            (Piece pieceF, PieceColor colorF, _) = Map[hF, vF];
            (_, PieceColor colorT, _) = Map[hT, vT];
            
            // Can't move same color
            if (colorF == colorT) return;

            // Move the piece
            Map[hT, vT] = (pieceF, colorF, MovedState.Moved);
            Map[hF, vF] = (Piece.Empty, PieceColor.None, MovedState.Unmoved);
            
            // Log the move
            Log.WriteToLog(from, to);

            if (pieceF != Piece.King) return;
            
            // Save king positions for fast fetching
            if (colorF == PieceColor.White) WhiteKing = to;
            else BlackKing = to;
        }
        
        public bool[,] BitBoard(PieceColor color)
        {
            bool[,] bitBoard = new bool[UBOUND, UBOUND];

            List<(int, int)> fromPieces = new(16);
            
            for (int h = 0; h < UBOUND; h++)
            for (int v = 0; v < UBOUND; v++) {
                // Add all pieces of the color
                if (Map[h, v].Item2 == color) fromPieces.Add((h, v));
                
                // Fill up Bitboard with default in same loop
                bitBoard[h, v] = false;
            }
            
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach ((int, int) from in fromPieces) {
                // Set verification off to avoid infinite recursion
                LegalMoveSet moveSet = new(this, from, false);
                (int, int)[] moves = moveSet.Get();

                foreach ((int h, int v) in moves) {
                    // Mark as valid for all true moves
                    bitBoard[h, v] = true;
                }
            }

            return bitBoard;
        }
        
        public void HighlightMoves((int, int) from)
        {
            LegalMoveSet set = new(this, from);
            HighlightedMoves = set.Get();
        }

        public override string ToString()
        {
            return DrawBoardCli().ToString().Trim(' ');
        }

        private Table DrawBoardCli()
        {
            TableBuilder builder = new(new CellFormat(Alignment.Center));
            
            // Add rank column
            builder.AddColumn("*");
            
            // Add columns for files
            for (int h = 0; h < UBOUND; h++) builder.AddColumn(((char)(65 + h)).ToString());

            Table table = builder.Build();

            for (int v = UBOUND - 1; v > LBOUND; v--) {
                // Count: Rank column + Files columns (1 + 8)
                ICell[] cells = new ICell[UBOUND + 1];
                for (int h = 0; h < UBOUND; h++) {
                    // Set rank column value
                    cells[0] = new TableCell((v + 1).ToString());

                    (Piece piece, PieceColor color, _) = Map[h, v];
                    string pieceRepresentation = piece switch
                    {
                        Piece.Empty => "   ",
                        Piece.Knight => "N",
                        _ => piece.ToString()[0].ToString()
                    };

                    Color uiColor = color switch
                    {
                        PieceColor.White => Color.AntiqueWhite,
                        PieceColor.Black => Color.Coral,
                        _ => Color.Gray
                    };

                    if (HighlightedMoves.Contains((h, v))) uiColor = piece == Piece.Empty ? Color.Yellow : Color.Red;

                    // Set piece value for file
                    cells[h + 1] = new TableCell(
                        pieceRepresentation,
                        new CellFormat(
                            fontStyle: FontStyleExt.Bold,
                            foregroundColor: Color.Black,
                            backgroundColor: uiColor,
                            alignment: Alignment.Center
                        )
                    );
                }

                // Add rank row
                table.AddRow(cells);
            }
            
            table.Config = TableConfig.Unicode();
            table.Config.hasInnerRows = true;

            HighlightedMoves = Array.Empty<(int, int)>();

            return table;
        }

    }

}