using System;
using System.Drawing;
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

        public const short UBOUND = 8;
        public const short LBOUND = -1;
        
        private const string DEFAULT_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private readonly BitBoardMap Map;

        private bool WhiteTurn;
        
        private bool WhiteKCastle;
        private bool WhiteQCastle;
        private bool BlackKCastle;
        private bool BlackQCastle;
        
        private BitBoard EnPassantTarget;
        private BitBoard HighlightedMoves = BitBoard.Default;
        
        public static DataBoard Default()
        {
            return FromFen(DEFAULT_FEN);
        }

        public static DataBoard FromFen(string fen)
        {
            string[] parts = fen.Split(" ");
            return new DataBoard(parts[0], parts[1], parts[2], parts[3]);
        }

        private DataBoard(DataBoard board)
        {
            Map = board.Map.Clone();
            
            WhiteTurn = board.WhiteTurn;

            WhiteKCastle = board.WhiteKCastle;
            WhiteQCastle = board.WhiteQCastle;
            BlackKCastle = board.BlackKCastle;
            BlackQCastle = board.BlackQCastle;

            EnPassantTarget = board.EnPassantTarget;
        }

        private DataBoard(string boardData, string turnData, string castlingData, string enPassantTargetData)
        {
            Map = new BitBoardMap(boardData);
            
            WhiteTurn = turnData[0] == 'w';
            
            WhiteKCastle = castlingData.Contains("K");
            WhiteQCastle = castlingData.Contains("Q");
            BlackKCastle = castlingData.Contains("k");
            BlackQCastle = castlingData.Contains("q");
            
            EnPassantTarget = BitBoard.Default;
            if (enPassantTargetData.Length == 2) {
                (int h, int v) = Util.ChessStringToTuple(enPassantTargetData.ToUpper());
                EnPassantTarget[h, v] = true;
            } else EnPassantTarget = BitBoard.Default;
        }

        internal BitBoard GetEnPassantTarget()
        {
            return EnPassantTarget;
        }
        
        public bool IsWhiteTurn()
        {
            return WhiteTurn;
        }
        
        public (bool, bool) CastlingRight(PieceColor color)
        {
            return color == PieceColor.White ? (WhiteQCastle, WhiteKCastle) : (BlackQCastle, BlackKCastle);
        }
        
        public (Piece, PieceColor) At((int, int) loc)
        {
            return Map[loc.Item1, loc.Item2];
        }
        
        public BitBoard All(PieceColor color)
        {
            return Map[color];
        }

        public BitBoard KingLoc(PieceColor color)
        {
            return Map[Piece.King, color];
        }

        public bool EmptyAt((int, int) loc)
        {
            return Map[loc.Item1, loc.Item2].Item1 == Piece.Empty;
        }

        public MoveAttempt SecureMove((int, int) from, (int, int) to)
        {
            LegalMoveSet moveSet = new(this, from);
            BitBoard moves = moveSet.Get();

            if (!moves[to.Item1, to.Item2]) return MoveAttempt.Fail;
            
            Move(from, to);

            PieceColor color = Map[to.Item1, to.Item2].Item2;
            PieceColor oppositeColor = Util.OppositeColor(color);

            LegalMoveSet opposingMoveSet = new(this, oppositeColor);
            if (opposingMoveSet.Count == 0) return MoveAttempt.Checkmate;

            BitBoard kingLoc = KingLoc(oppositeColor);
            return AttackBitBoard(color) & kingLoc ? MoveAttempt.SuccessAndCheck : MoveAttempt.Success;
        }

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
            
            if (EnPassantTarget && to == EnPassantTarget && pieceF == Piece.Pawn) {
                int vA = colorF == PieceColor.White ? vT - 1 : vT + 1;
                Map.Empty(hT, vA);
            }

            if (pieceF == Piece.Pawn && Math.Abs(vT - vF) == 2) {
                EnPassantTarget = colorF == PieceColor.White ? (hF, vT - 1) : (hF, vT + 1);
            } else EnPassantTarget = BitBoard.Default;
            
            Map.Move(from, to);

            // Castling and castling right update on rook move
            switch (pieceF) {
                case Piece.King when Math.Abs(hT - hF) == 2:
                    if (hT > hF) { // King side castle
                        Map.Move((7, vF), (hT - 1, vF));
                    } else { // Queen side castle
                        Map.Move((0, vF), (hT + 1, vF));
                    }

                    break;
                case Piece.Rook:
                    switch (colorF) {
                        case PieceColor.White:
                            switch (hF) {
                                case 0:
                                    WhiteQCastle = false;
                                    break;
                                case 7:
                                    WhiteKCastle = false;
                                    break;
                            }

                            break;
                        case PieceColor.Black:
                            switch (hF) {
                                case 0:
                                    BlackQCastle = false;
                                    break;
                                case 7:
                                    BlackKCastle = false;
                                    break;
                            }

                            break;
                        case PieceColor.None:
                        default:
                            throw new InvalidOperationException("Rook cannot have no color.");
                    }

                    break;
                case Piece.Empty:
                case Piece.Pawn:
                case Piece.Knight:
                case Piece.Bishop:
                case Piece.Queen:
                default:
                    break;
            }

            // Castling right update on king move
            if (pieceF == Piece.King) {
                switch (colorF) {
                    case PieceColor.White:
                        WhiteKCastle = false;
                        WhiteQCastle = false;
                        break;
                    case PieceColor.Black:
                        BlackKCastle = false;
                        BlackQCastle = false;
                        break;
                    case PieceColor.None:
                    default:
                        throw new InvalidOperationException("King cannot have no color.");
                }
            }

            // Castling right update on rook captured
            // ReSharper disable once InvertIf
            if (pieceT == Piece.Rook) {
                switch (colorT) {
                    case PieceColor.White:
                        if (hT == 7) WhiteKCastle = false;
                        else WhiteQCastle = false;
                        break;
                    case PieceColor.Black:
                        if (hT == 7) BlackKCastle = false;
                        else BlackQCastle = false;
                        break;
                    case PieceColor.None:
                    default:
                        throw new InvalidOperationException("Rook cannot have no color.");
                }
            }

            WhiteTurn = !WhiteTurn;
        }

        public DataBoard Clone()
        {
            return new DataBoard(this);
        }

        public BitBoard AttackBitBoard(PieceColor color)
        {
            BitBoard attackBoard = BitBoard.Default;
            BitBoard colored = Map[color];
            foreach ((int h, int v) in colored) {
                LegalMoveSet moveSet = new(this, (h, v), false);
                attackBoard |= moveSet.Get();
            }

            return attackBoard;
        }

        public void HighlightMoves((int, int) from)
        {
            LegalMoveSet moveSet = new(this, from);
            HighlightedMoves = moveSet.Get();
        }

        public void HighlightMoves(PieceColor color)
        {
            if (color == PieceColor.None) 
                throw new InvalidOperationException("Cannot highlight moves for no color.");

            LegalMoveSet moveSet = new(this, color);
            HighlightedMoves = moveSet.Get();
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
            if (!WhiteKCastle && !WhiteQCastle && !BlackKCastle && !BlackQCastle) {
                castlingRight = "-";
                goto EnPassantFill;
            }
            
            if (WhiteKCastle) castlingRight += "K";
            if (WhiteQCastle) castlingRight += "Q";
            if (BlackKCastle) castlingRight += "k";
            if (BlackQCastle) castlingRight += "q";
            
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