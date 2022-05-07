using System;
using System.Drawing;
using Backend.Exception;
using BetterConsoles.Core;
using BetterConsoles.Tables;
using BetterConsoles.Tables.Builders;
using BetterConsoles.Tables.Configuration;
using BetterConsoles.Tables.Models;

namespace Backend.Board
{

    public class BitDataBoard
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
        
        public static BitDataBoard Default()
        {
            return FromFen(DEFAULT_FEN);
        }

        public static BitDataBoard FromFen(string fen)
        {
            string[] parts = fen.Split(" ");
            return new BitDataBoard(parts[0], parts[1], parts[2], parts[3]);
        }

        private BitDataBoard(BitDataBoard board)
        {
            Map = board.Map.Clone();
            
            WhiteTurn = board.WhiteTurn;

            WhiteKCastle = board.WhiteKCastle;
            WhiteQCastle = board.WhiteQCastle;
            BlackKCastle = board.BlackKCastle;
            BlackQCastle = board.BlackQCastle;

            EnPassantTarget = board.EnPassantTarget;
        }

        private BitDataBoard(string boardData, string turnData, string castlingData, string enPassantTargetData)
        {
            Map = new BitBoardMap(boardData);
            
            WhiteTurn = turnData[0] == 'w';
            
            WhiteKCastle = castlingData.Contains("K");
            WhiteQCastle = castlingData.Contains("Q");
            BlackKCastle = castlingData.Contains("k");
            BlackQCastle = castlingData.Contains("q");
            
            if (enPassantTargetData.Length == 2) {
                (int h, int v) = Util.ChessStringToTuple(enPassantTargetData.ToUpper());
                EnPassantTarget = new BitBoard(BitBoard.Default)
                {
                    [h, v] = true
                };
            } else EnPassantTarget = BitBoard.Default;
        }

        internal BitBoardMap GetMap()
        {
            return Map;
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

        public void Move((int, int) from, (int, int) to)
        {
            (int hF, int vF) = from;
            (int hT, int vT) = to;

            if (hT is < 0 or >= UBOUND || vT is < 0 or >= UBOUND)
                throw new InvalidOperationException("Cannot move to " + Util.TupleToChessString(to) + ".");

            (Piece pieceF, PieceColor colorF) = Map[hF, vF];
            (Piece pieceT, PieceColor colorT) = Map[hT, vT];
            
            // Can't move same color.
            if (colorF == colorT) return;
            // throw InvalidMoveAttemptException.FromBoard(this, Log, "Cannot move to same color.");
            
            if (EnPassantTarget && to == EnPassantTarget && pieceF == Piece.Pawn) {
                int vA = colorF == PieceColor.White ? vT - 1 : vT + 1;
                Map.Empty(hT, vA);
            }

            if (pieceF == Piece.Pawn && Math.Abs(vT - vF) == 2) {
                EnPassantTarget = colorF == PieceColor.White ? (hF, vT - 1) : (hF, vT + 1);
            } else EnPassantTarget = BitBoard.Default;
            
            Map.Move(from, to);

            // Castling and castling right update on king/rook move
            switch (pieceF) {
                case Piece.King when Math.Abs(hT - hF) == 2:
                {
                    if (hT > hF) { // King side castle
                        Map.Move((7, vF), (hT - 1, vF));
                    } else { // Queen side castle
                        Map.Move((0, vF), (hT + 1, vF));
                    }

                    break;
                }
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
                case Piece.King:
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
                    break;
                case Piece.Empty:
                case Piece.Pawn:
                case Piece.Knight:
                case Piece.Bishop:
                case Piece.Queen:
                default:
                    break;
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
        }

        public BitDataBoard Clone()
        {
            return new BitDataBoard(this);
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

                    // if (HighlightedMoves.Contains((h, v))) {
                    //     uiColor = piece == Piece.Empty ? Color.Yellow : Color.Red;
                    //     if (piece == Piece.Empty && EnPassantTarget.HasValue && (h, v) == EnPassantTarget.Value) 
                    //         uiColor = Color.Red; 
                    // }

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

            // HighlightedMoves = Array.Empty<(int, int)>();

            return table;
        }

    }

}