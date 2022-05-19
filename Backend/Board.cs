using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Data.Move;
using Backend.Data.Struct;
using Backend.Exception;
using BetterConsoles.Core;
using BetterConsoles.Tables;
using BetterConsoles.Tables.Builders;
using BetterConsoles.Tables.Configuration;
using BetterConsoles.Tables.Models;

namespace Backend
{

    public class Board
    {

        public const short UBOUND = 8;
        public const short LBOUND = -1;
        
        private const string DEFAULT_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public bool WhiteTurn => Map.WhiteTurn;
        
        private BitBoardMap Map;

        private BoardHistoryStack History;

        private BitBoard HighlightedMoves = BitBoard.Default;
        
        internal BitBoard EnPassantTarget => Map.EnPassantTarget;
        
        public static Board Default()
        {
            return FromFen(DEFAULT_FEN);
        }

        public static Board FromFen(string fen)
        {
            string[] parts = fen.Split(" ");
            return new Board(parts[0], parts[1], parts[2], parts[3]);
        }

        private Board(Board board)
        {
            Map = board.Map;
            History = board.History.Clone();
        }

        private Board(string boardData, string turnData, string castlingData, string enPassantTargetData)
        {
            Map = new BitBoardMap(boardData, turnData, castlingData, enPassantTargetData);
            
            History = new BoardHistoryStack(256);
        }
        
        public (bool, bool) CastlingRight(PieceColor color) => color == PieceColor.White ? 
            (Map.WhiteQCastle, Map.WhiteKCastle) : (Map.BlackQCastle, Map.BlackKCastle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (Piece, PieceColor) At((int, int) loc) => Map[loc.Item1, loc.Item2];
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard All(PieceColor color) => Map[color];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard All(Piece piece, PieceColor color) => Map[piece, color];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard KingLoc(PieceColor color) => Map[Piece.King, color];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EmptyAt((int, int) loc) => Map[loc.Item1, loc.Item2].Item1 == Piece.Empty;

        public MoveAttempt SecureMove((int, int) from, (int, int) to)
        {
            MoveList moveList = new(this, from);
            BitBoard moves = moveList.Get();

            if (!moves[to.Item1, to.Item2]) return MoveAttempt.Fail;
            
            Move(from, to);

            PieceColor color = Map[to.Item1, to.Item2].Item2;
            PieceColor oppositeColor = Util.OppositeColor(color);

            MoveList opposingMoveList = new(this, oppositeColor);
            if (opposingMoveList.Count == 0) return MoveAttempt.Checkmate;

            BitBoard kingLoc = KingLoc(oppositeColor);
            return MoveList.UnderAttack(this, kingLoc, color) ? 
                MoveAttempt.SuccessAndCheck : MoveAttempt.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Move((int, int) from, (int, int) to)
        {
            (int hF, int vF) = from;
            (int hT, int vT) = to;

            if (hT is < 0 or >= UBOUND || vT is < 0 or >= UBOUND)
                throw new InvalidOperationException("Cannot move to " + Util.TupleToChessString(to) + ".");

            (Piece pieceF, PieceColor colorF) = Map[hF, vF];
            (Piece pieceT, PieceColor colorT) = Map[hT, vT];
            
            // Can't move same color.
            if (colorF == colorT) {
                throw InvalidMoveAttemptException.FromBoard(this, "Cannot move to same color.");
            }
            
            History.Push(Map);
            
            if (EnPassantTarget && to == EnPassantTarget && pieceF == Piece.Pawn) {
                int vA = colorF == PieceColor.White ? vT - 1 : vT + 1;
                Map.Empty(hT, vA);
            }

            if (pieceF == Piece.Pawn && Math.Abs(vT - vF) == 2) {
                Map.EnPassantTarget = colorF == PieceColor.White ? (hF, vT - 1) : (hF, vT + 1);
            } else Map.EnPassantTarget = BitBoard.Default;
            
            Map.Move(from, to);

            switch (pieceF) {
                // Castling rights update on rook move
                case Piece.Rook:
                    switch (colorF) {
                        case PieceColor.White:
                            switch (hF) {
                                case 0:
                                    Map.WhiteQCastle = false;
                                    break;
                                case 7:
                                    Map.WhiteKCastle = false;
                                    break;
                            }

                            break;
                        case PieceColor.Black:
                            switch (hF) {
                                case 0:
                                    Map.BlackQCastle = false;
                                    break;
                                case 7:
                                    Map.BlackKCastle = false;
                                    break;
                            }

                            break;
                        case PieceColor.None:
                        default:
                            throw new InvalidOperationException("Rook cannot have no color.");
                    }

                    break;
                // Castling rights update on king move & castling
                case Piece.King:
                {
                    switch (colorF) {
                        case PieceColor.White:
                            Map.WhiteKCastle = false;
                            Map.WhiteQCastle = false;
                            break;
                        case PieceColor.Black:
                            Map.BlackKCastle = false;
                            Map.BlackQCastle = false;
                            break;
                        case PieceColor.None:
                        default:
                            throw new InvalidOperationException("King cannot have no color.");
                    }

                    // Castling.
                    int d = Math.Abs(hT - hF);
                    if (d == 2) {
                        if (hT > hF) Map.Move((7, vF), (hT - 1, vF)); // King-side
                        else Map.Move((0, vF), (hT + 1, vF)); // Queen-side
                    }

                    break;
                }
                case Piece.Empty:
                case Piece.Pawn:
                case Piece.Knight:
                case Piece.Bishop:
                case Piece.Queen:
                default:
                    break;
            }

            // Castling right update on rook captured
            if (pieceT == Piece.Rook) {
                switch (colorT) {
                    case PieceColor.White:
                        if (hT == 7) Map.WhiteKCastle = false;
                        else Map.WhiteQCastle = false;
                        break;
                    case PieceColor.Black:
                        if (hT == 7) Map.BlackKCastle = false;
                        else Map.BlackQCastle = false;
                        break;
                    case PieceColor.None:
                    default:
                        throw new InvalidOperationException("Rook cannot have no color.");
                }
            }

            Map.WhiteTurn = !WhiteTurn;
        }

        public void UndoMove()
        {
            if (History.Count < 1) return;

            Map = History.Pop();
        }

        public Board Clone()
        {
            return new Board(this);
        }

        public void HighlightMoves((int, int) from)
        {
            MoveList moveList = new(this, from);
            HighlightedMoves = moveList.Get();
        }

        public void HighlightMoves(PieceColor color)
        {
            if (color == PieceColor.None) 
                throw new InvalidOperationException("Cannot highlight moves for no color.");

            MoveList moveList = new(this, color);
            HighlightedMoves = moveList.Get();
        }

        public override string ToString()
        {
            string board = DrawBoardCli().ToString().Trim(' ');
            string fen = "FEN: " + GenerateFen() + "\n";
            return board + fen;
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

                    if (HighlightedMoves[h, v]) {
                        uiColor = piece == Piece.Empty ? Color.Yellow : Color.Red;
                        if (piece == Piece.Empty && EnPassantTarget && EnPassantTarget[h, v]) 
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

            HighlightedMoves = BitBoard.Default;

            return table;
        }

        private string GenerateFen()
        {
            string boardData = Map.GenerateBoardFen();
            string turnData = WhiteTurn ? "w" : "b";
            
            string castlingRight = "";
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (!Map.WhiteKCastle && !Map.WhiteQCastle && !Map.BlackKCastle && !Map.BlackQCastle) {
                castlingRight = "-";
                goto EnPassantFill;
            }
            
            if (Map.WhiteKCastle) castlingRight += "K";
            if (Map.WhiteQCastle) castlingRight += "Q";
            if (Map.BlackKCastle) castlingRight += "k";
            if (Map.BlackQCastle) castlingRight += "q";
            
            EnPassantFill:
            string enPassantTarget = "-";
            if (EnPassantTarget) {
                enPassantTarget = Util.TupleToChessString(((int, int))EnPassantTarget).ToLower();
            }

            string[] fen = { boardData, turnData, castlingRight, enPassantTarget };
            return string.Join(" ", fen);
        }

    }

}