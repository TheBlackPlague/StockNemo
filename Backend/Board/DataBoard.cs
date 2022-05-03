using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

        private const string DEFAULT_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public const int UBOUND = 8; // Board Upper Bound
        public const int LBOUND = -1; // Board Lower Bound

        private readonly (Piece, PieceColor)[,] Map = new(Piece, PieceColor)[UBOUND, UBOUND];
        private readonly List<(int, int)> White = new(16);
        private readonly List<(int, int)> Black = new(16);
        private readonly List<(int, int)>[] Colored = new List<(int, int)>[2]; 

        private readonly Log Log = new();

        private bool WhiteTurn = true;

        private bool WhiteKCastle = true;
        private bool WhiteQCastle = true;
        private bool BlackKCastle = true;
        private bool BlackQCastle = true;

        private (int, int) WhiteKing;
        private (int, int) BlackKing;
        private (int, int)? EnPassantTarget;

        private (int, int)[] HighlightedMoves = Array.Empty<(int, int)>();
        
        public static DataBoard Default()
        {
            return FromFen(DEFAULT_FEN);
        }

        public static DataBoard FromFen(string fen)
        {
            string[] parts = fen.Split(" ");
            return new DataBoard(parts[0], parts[1], parts[2], parts[3]);
        }

        private DataBoard() {}

        private DataBoard(string boardData, string turnData, string castlingData, string enPassantTargetData)
        {
            string[] expandedBoardData = boardData.Split("/").Reverse().ToArray();
            if (expandedBoardData.Length != UBOUND) 
                throw new InvalidDataException("Wrong board data provided: " + boardData);
            
            for (int v = 0; v < UBOUND; v++) {
                string rankData = expandedBoardData[v];
                int h = 0;
                foreach (char p in rankData) {
                    if (char.IsNumber(p)) {
                        int emptyC = int.Parse(p.ToString());
                        for (int hI = h; hI < h + emptyC; hI++) {
                            Map[hI, v] = Util.EmptyPieceState();
                        }

                        h += emptyC;
                        continue;
                    }
                    
                    Piece piece = p switch
                    {
                        'p' or 'P' => Piece.Pawn,
                        'r' or 'R' => Piece.Rook,
                        'n' or 'N' => Piece.Knight,
                        'b' or 'B' => Piece.Bishop,
                        'q' or 'Q' => Piece.Queen,
                        'k' or 'K' => Piece.King,
                        _ => throw new InvalidDataException("Piece " + p + " isn't recognized.")
                    };
                    PieceColor color = char.IsUpper(p) ? PieceColor.White : PieceColor.Black;

                    Map[h, v] = (piece, color);

                    (int, int) position;
                    switch (color) {
                        case PieceColor.White:
                            position = (h, v);
                            White.Add(position);
                            if (piece == Piece.King) WhiteKing = (h, v);
                            break;
                        case PieceColor.Black:
                            position = (h, v);
                            Black.Add(position);
                            if (piece == Piece.King) BlackKing = (h, v);
                            break;
                        case PieceColor.None:
                        default:
                            throw new InvalidOperationException("Unreachable statement reached.");
                    }
                    
                    h++;
                }
            }

            Colored[(int)PieceColor.White] = White;
            Colored[(int)PieceColor.Black] = Black;

            WhiteTurn = turnData[0] == 'w';

            WhiteKCastle = castlingData.Contains("K");
            WhiteQCastle = castlingData.Contains("Q");
            BlackKCastle = castlingData.Contains("k");
            BlackQCastle = castlingData.Contains("q");

            if (enPassantTargetData.Length == 2) {
                EnPassantTarget = Util.ChessStringToTuple(enPassantTargetData.ToUpper());
            } else EnPassantTarget = null;
        }

        public DataBoard Clone()
        {
            DataBoard board = new();

            Array.Copy(Map, board.Map, 64);
            
            board.White.AddRange(White);
            board.Black.AddRange(Black);
            board.Colored[(int)PieceColor.White] = board.White;
            board.Colored[(int)PieceColor.Black] = board.Black;
            
            board.WhiteKing = WhiteKing;
            board.BlackKing = BlackKing;

            board.WhiteTurn = WhiteTurn;
            board.WhiteKCastle = WhiteKCastle;
            board.WhiteQCastle = WhiteQCastle;
            board.BlackKCastle = BlackKCastle;
            board.BlackQCastle = BlackQCastle;

            board.EnPassantTarget = EnPassantTarget;

            return board;
        }

        public int MoveCount()
        {
            return Log.Count();
        }

        public bool IsWhiteTurn()
        {
            return WhiteTurn;
        }

        public (int, int)? GetEnPassantTarget()
        {
            return EnPassantTarget;
        }

        public (Piece, PieceColor) At((int, int) loc)
        {
            if (loc.Item1 is < 0 or >= UBOUND || loc.Item2 is < 0 or >= UBOUND)
                throw new InvalidOperationException("Cannot locate: " + loc + ".");
            return Map[loc.Item1, loc.Item2];
        }
        
        public (int, int)[] All(PieceColor color)
        {
            return color == PieceColor.None ? Array.Empty<(int, int)>() : Colored[(int)color].ToArray();
        }

        public (int, int) KingLoc(PieceColor color)
        {
            return color == PieceColor.White ? WhiteKing : BlackKing;
        }

        public bool EmptyAt((int, int) loc)
        {
            if (loc.Item1 is < 0 or >= UBOUND || loc.Item2 is < 0 or >= UBOUND)
                throw new InvalidOperationException("Cannot locate: " + loc + ".");
            return Map[loc.Item1, loc.Item2].Item1 == Piece.Empty;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public MoveAttempt SecureMove((int, int) from, (int, int) to)
        {
            LegalMoveSet moveSet = new(this, from);
            (int, int)[] moves = moveSet.Get();

            if (!moves.Contains(to)) return MoveAttempt.Fail;

            Move(from, to);
            
            // Log the move.
            Log.WriteToLog(from, to);

            PieceColor color = Map[to.Item1, to.Item2].Item2;
            PieceColor oppositeColor = Util.OppositeColor(color);

            LegalMoveSet opposingMoves = new(this, oppositeColor);
            if (opposingMoves.Count() == 0) return MoveAttempt.Checkmate;

            (int, int) kingLoc = KingLoc(oppositeColor);
            return AttackBitBoard(color)[kingLoc.Item1, kingLoc.Item2] ? MoveAttempt.SuccessAndCheck : MoveAttempt.Success;
        }

        public void Move((int, int) from, (int, int) to)
        {
            (int hF, int vF) = from;
            (int hT, int vT) = to;

            if (hT is < 0 or >= UBOUND || vT is < 0 or >= UBOUND)
                throw new InvalidOperationException("Cannot move to " + Util.TupleToChessString(to) + ".");

            (Piece pieceF, PieceColor colorF) = Map[hF, vF];
            (_, PieceColor colorT) = Map[hT, vT];

            // Can't move same color
            if (colorF == colorT) 
                throw InvalidMoveAttemptException.FromBoard(this, Log, "Cannot move to same color.");

            // Generate updated piece state
            (Piece, PieceColor) pieceState = (pieceF, colorF);
            
            // En Passant Capture
            if (EnPassantTarget.HasValue && to == EnPassantTarget.Value && pieceF == Piece.Pawn) {
                int vA = colorF == PieceColor.White ? vT - 1 : vT + 1;
                Map[hT, vA] = Util.EmptyPieceState();
                Colored[(int)Util.OppositeColor(colorF)].Remove((hT, vA));
            }

            if (pieceF == Piece.Pawn && Math.Abs(vT - vF) == 2) {
                EnPassantTarget = colorF == PieceColor.Black ? (hF, vT + 1) : (hF, vT - 1);
            } else EnPassantTarget = null;

            // Update map
            Map[hT, vT] = pieceState;
            Map[hF, vF] = Util.EmptyPieceState();
            if (colorT is PieceColor.White or PieceColor.Black) {
                Colored[(int)colorT].Remove(to);
            }

            Colored[(int)colorF].Remove(from);
            Colored[(int)colorF].Add(to);
            
            WhiteTurn = !WhiteTurn;
            
            if (pieceF != Piece.King) return;
            
            // Save king positions for fast fetching
            if (colorF == PieceColor.White) WhiteKing = to;
            else BlackKing = to;
        }
        
        public bool[,] AttackBitBoard(PieceColor color)
        {
            bool[,] bitBoard = new bool[UBOUND, UBOUND];

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach ((int, int) from in Colored[(int)color]) {
                // Set verification off to avoid infinite recursion
                LegalMoveSet moveSet = new(this, from, false);

                foreach ((int h, int v) in moveSet.Get()) {
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

                    (Piece piece, PieceColor color) = Map[h, v];
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

                    if (HighlightedMoves.Contains((h, v))) {
                        uiColor = piece == Piece.Empty ? Color.Yellow : Color.Red;
                        if (piece == Piece.Empty && EnPassantTarget.HasValue && (h, v) == EnPassantTarget.Value) 
                            uiColor = Color.Red; 
                    }

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