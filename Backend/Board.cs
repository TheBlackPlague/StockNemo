using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;
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

        private BitBoard HighlightedMoves = BitBoard.Default;
        
        internal Square EnPassantTarget => Map.EnPassantTarget;
        
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
            Map = board.Map.Copy();
        }

        private Board(string boardData, string turnData, string castlingData, string enPassantTargetData)
        {
            Map = new BitBoardMap(boardData, turnData, castlingData, enPassantTargetData);
        }

        #region Readonly Properties
        
        public BitBoardMap GetCurrentState => Map;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool) CastlingRight(PieceColor color) => color == PieceColor.White ? 
            (Map.WhiteQCastle, Map.WhiteKCastle) : (Map.BlackQCastle, Map.BlackKCastle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (Piece, PieceColor) At(Square sq) => Map[sq];
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard All(PieceColor color) => Map[color];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard All(Piece piece, PieceColor color) => Map[piece, color];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard KingLoc(PieceColor color) => Map[Piece.King, color];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EmptyAt(Square sq) => Map[sq].Item1 == Piece.Empty;
        
        #endregion

        #region Move
        
        public MoveAttempt SecureMove(Square from, Square to)
        {
            MoveList moveList = new(this, from);
            BitBoard moves = moveList.Get();

            if (!moves[to]) return MoveAttempt.Fail;
            
            Move(from, to);

            PieceColor color = Map[to].Item2;
            PieceColor oppositeColor = Util.OppositeColor(color);

            MoveList opposingMoveList = new(this, oppositeColor);
            if (opposingMoveList.Count == 0) return MoveAttempt.Checkmate;

            BitBoard kingLoc = KingLoc(oppositeColor);
            return MoveList.UnderAttack(this, kingLoc, color) ? 
                MoveAttempt.SuccessAndCheck : MoveAttempt.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public RevertMove Move(Square from, Square to)
        {
            (Piece pieceF, PieceColor colorF) = Map[from];
            (Piece pieceT, PieceColor colorT) = Map[to];
            
            // Can't move same color.
            if (colorF == colorT) {
                throw InvalidMoveAttemptException.FromBoard(this, "Cannot move to same color.");
            }
            
            RevertMove rv = RevertMove.FromBitBoardMap(ref Map);
            if (pieceT != Piece.Empty) {
                rv.CapturedPiece = pieceT;
                rv.CapturedColor = colorT;
            }

            if (EnPassantTarget == to && pieceF == Piece.Pawn) {
                Square epPiece = colorF == PieceColor.White ? EnPassantTarget - 8 : EnPassantTarget + 8;
                Map.Empty(epPiece);

                rv.EnPassant = true;
                rv.CapturedColor = Util.OppositeColor(colorF);
            }

            if (pieceF == Piece.Pawn && Math.Abs(to - from) == 16) {
                Map.EnPassantTarget = colorF == PieceColor.White ? from + 8 : from - 8;
            } else Map.EnPassantTarget = Square.Na;

            Map.Move(from, to);

            rv.From = from;
            rv.To = to;

            switch (pieceF) {
                // Castling rights update on rook move
                case Piece.Rook:
                    switch (colorF) {
                        case PieceColor.White:
                            switch ((int)from % 8) {
                                case 0:
                                    Map.WhiteQCastle = false;
                                    break;
                                case 7:
                                    Map.WhiteKCastle = false;
                                    break;
                            }

                            break;
                        case PieceColor.Black:
                            switch ((int)from % 8) {
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
                    int d = Math.Abs(to - from);
                    if (d == 2) {
                        if (to > from) { // King-side
                            rv.SecondaryFrom = to + 1;
                            rv.SecondaryTo = to - 1;
                        } else { // Queen-side
                            rv.SecondaryFrom = to - 2;
                            rv.SecondaryTo = to + 1;
                        }
                        
                        Map.Move(rv.SecondaryFrom, rv.SecondaryTo);
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
                        if ((int)to % 8 == 7) Map.WhiteKCastle = false;
                        else Map.WhiteQCastle = false;
                        break;
                    case PieceColor.Black:
                        if ((int)to % 8 == 7) Map.BlackKCastle = false;
                        else Map.BlackQCastle = false;
                        break;
                    case PieceColor.None:
                    default:
                        throw new InvalidOperationException("Rook cannot have no color.");
                }
            }

            Map.WhiteTurn = !WhiteTurn;

            return rv;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void UndoMove(ref RevertMove rv)
        {
            // Castling rights
            Map.WhiteKCastle = rv.WhiteKCastle;
            Map.WhiteQCastle = rv.WhiteQCastle;
            Map.BlackKCastle = rv.BlackKCastle;
            Map.BlackQCastle = rv.BlackQCastle;
            
            // EP target
            Map.EnPassantTarget = rv.EnPassantTarget;
            
            // Flip turns
            Map.WhiteTurn = rv.WhiteTurn;

            // Move piece back
            Map.Move(rv.To, rv.From);
            
            // If it was an E.P move, reset target and insert pawn
            if (rv.EnPassant) {
                Square insertion = rv.CapturedColor == PieceColor.White ? rv.To + 8 : rv.To - 8;
                Map.InsertPiece(insertion, Piece.Pawn, rv.CapturedColor);
                
                return;
            }

            // If capture happened, insert piece at captured location
            if (rv.CapturedPiece != Piece.Empty) {
                Map.InsertPiece(rv.To, rv.CapturedPiece, rv.CapturedColor);
                
                return;
            }

            // If there was a secondary move (castling), revert the secondary move.
            if (rv.SecondaryFrom != Square.Na) Map.Move(rv.SecondaryTo, rv.SecondaryFrom);
        }
        
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Board Clone()
        {
            return new Board(this);
        }

        public void HighlightMoves(Square from)
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
                cells[0] = new TableCell((v + 1).ToString());
                for (int h = 0; h < UBOUND; h++) {
                    Square sq = (Square)(v * 8 + h);
                    (Piece piece, PieceColor color) = Map[sq];
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

                    if (HighlightedMoves[sq]) {
                        uiColor = piece == Piece.Empty ? Color.Yellow : Color.Red;
                        if (piece == Piece.Empty && sq == EnPassantTarget) 
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
            if (EnPassantTarget != Square.Na) {
                enPassantTarget = EnPassantTarget.ToString().ToLower();
            }

            string[] fen = { boardData, turnData, castlingRight, enPassantTarget };
            return string.Join(" ", fen);
        }

    }

}