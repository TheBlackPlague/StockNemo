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

        public bool WhiteTurn => State.WhiteTurn;
        
        private BitBoardMap Map;
        private BoardState State;

        private BoardHistoryStack History;

        private BitBoard HighlightedMoves = BitBoard.Default;
        
        internal BitBoard EnPassantTarget => State.EnPassantTarget;
        
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
            Map = board.Map;
            State = board.State;
            History = board.History;
        }

        private DataBoard(string boardData, string turnData, string castlingData, string enPassantTargetData)
        {
            Map = new BitBoardMap(boardData);

            State = new BoardState
            {
                WhiteTurn = turnData[0] == 'w',
                WhiteKCastle = castlingData.Contains("K"),
                WhiteQCastle = castlingData.Contains("Q"),
                BlackKCastle = castlingData.Contains("k"),
                BlackQCastle = castlingData.Contains("q"),
                EnPassantTarget = BitBoard.Default
            };

            if (enPassantTargetData.Length == 2) {
                (int h, int v) = Util.ChessStringToTuple(enPassantTargetData.ToUpper());
                State.EnPassantTarget[h, v] = true;
            }

            History = new BoardHistoryStack(512);
        }
        
        public (bool, bool) CastlingRight(PieceColor color) => color == PieceColor.White ? 
            (State.WhiteQCastle, State.WhiteKCastle) : (State.BlackQCastle, State.BlackKCastle);

        public (Piece, PieceColor) At((int, int) loc) => Map[loc.Item1, loc.Item2];
        
        public BitBoard All(PieceColor color) => Map[color];

        public BitBoard All(Piece piece, PieceColor color) => Map[piece, color];

        public BitBoard KingLoc(PieceColor color) => Map[Piece.King, color];

        public bool EmptyAt((int, int) loc) => Map[loc.Item1, loc.Item2].Item1 == Piece.Empty;

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
            return LegalMoveSet.UnderAttack(this, kingLoc, color) ? 
                MoveAttempt.SuccessAndCheck : MoveAttempt.Success;
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

            BoardState lastBoardState = State;
            (int, int) capturedAt = to;
            
            if (EnPassantTarget && to == EnPassantTarget && pieceF == Piece.Pawn) {
                int vA = colorF == PieceColor.White ? vT - 1 : vT + 1;
                Map.Empty(hT, vA);
                capturedAt = (hT, vA);
            }

            if (pieceF == Piece.Pawn && Math.Abs(vT - vF) == 2) {
                State.EnPassantTarget = colorF == PieceColor.White ? (hF, vT - 1) : (hF, vT + 1);
            } else State.EnPassantTarget = BitBoard.Default;
            
            Map.Move(from, to);

            switch (pieceF) {
                // Castling rights update on rook move
                case Piece.Rook:
                    switch (colorF) {
                        case PieceColor.White:
                            switch (hF) {
                                case 0:
                                    State.WhiteQCastle = false;
                                    break;
                                case 7:
                                    State.WhiteKCastle = false;
                                    break;
                            }

                            break;
                        case PieceColor.Black:
                            switch (hF) {
                                case 0:
                                    State.BlackQCastle = false;
                                    break;
                                case 7:
                                    State.BlackKCastle = false;
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
                            State.WhiteKCastle = false;
                            State.WhiteQCastle = false;
                            break;
                        case PieceColor.Black:
                            State.BlackKCastle = false;
                            State.BlackQCastle = false;
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
                        if (hT == 7) State.WhiteKCastle = false;
                        else State.WhiteQCastle = false;
                        break;
                    case PieceColor.Black:
                        if (hT == 7) State.BlackKCastle = false;
                        else State.BlackQCastle = false;
                        break;
                    case PieceColor.None:
                    default:
                        throw new InvalidOperationException("Rook cannot have no color.");
                }
            }

            State.WhiteTurn = !WhiteTurn;

            MoveState moveState = new()
            {
                From = from,
                To = to,
            };

            if (pieceT != Piece.Empty) moveState.Captured = (pieceT, colorT, capturedAt);
            
            History.Push((lastBoardState, moveState));
        }

        public void UndoMove()
        {
            if (History.Count < 1) return;

            (BoardState lastState, MoveState moveState) = History.Pop();
            State = lastState;
            Map.Move(moveState.To, moveState.From);
            if (moveState.Captured.HasValue) Map.InsertPiece(
                moveState.Captured.Value.Item1, 
                moveState.Captured.Value.Item2, 
                moveState.Captured.Value.Item3
            );
        }

        public DataBoard Clone()
        {
            return new DataBoard(this);
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
            if (!State.WhiteKCastle && !State.WhiteQCastle && !State.BlackKCastle && !State.BlackQCastle) {
                castlingRight = "-";
                goto EnPassantFill;
            }
            
            if (State.WhiteKCastle) castlingRight += "K";
            if (State.WhiteQCastle) castlingRight += "Q";
            if (State.BlackKCastle) castlingRight += "k";
            if (State.BlackQCastle) castlingRight += "q";
            
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