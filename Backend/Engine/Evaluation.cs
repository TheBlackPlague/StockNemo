using System.Runtime.CompilerServices;
using Backend.Data;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend.Engine;

public static class Evaluation
{
    
    private const int QUEEN = 900;
    private const int ROOK = 500;
    private const int BISHOP_KNIGHT = 300;
    private const int PAWN = 100;

    public static readonly PieceDevelopmentTable PieceDevelopmentTable = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RelativeEvaluation(Board board)
    {
        return (Material(board) + board.PieceDevelopmentEvaluation) * (board.WhiteTurn ? 1 : -1);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int InitialPieceDevelopmentEvaluation(ref BitBoardMap map)
    {
        int whiteScore = 0;
        int blackScore = 0;

        #region White Score Calculation

        Piece piece = Piece.Pawn;
        while (piece != Piece.Empty) {
            BitBoardIterator pieceIterator = map[piece, PieceColor.White].GetEnumerator();
            Square pieceSq = pieceIterator.Current;
            while (pieceIterator.MoveNext()) {
                whiteScore += PieceDevelopmentTable[piece, (Square)((byte)pieceSq ^ 56)];
                pieceSq = pieceIterator.Current;
            }
            piece++;
        }

        #endregion

        #region Black Score Calculation

        piece = Piece.Pawn;
        while (piece != Piece.Empty) {
            BitBoardIterator pieceIterator = map[piece, PieceColor.Black].GetEnumerator();
            Square pieceSq = pieceIterator.Current;
            while (pieceIterator.MoveNext()) {
                blackScore += PieceDevelopmentTable[piece, pieceSq];
                pieceSq = pieceIterator.Current;
            }
            piece++;
        }

        #endregion

        return whiteScore - blackScore;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static int Material(Board board)
    {
        // First, we must calculate number of white pieces (with consideration to type of piece) relative to the pieces
        // of black.
        int dQ = board.All(Piece.Queen, PieceColor.White).Count - board.All(Piece.Queen, PieceColor.Black).Count;
        int dB = board.All(Piece.Bishop, PieceColor.White).Count - board.All(Piece.Bishop, PieceColor.Black).Count;
        int dN = board.All(Piece.Knight, PieceColor.White).Count - board.All(Piece.Knight, PieceColor.Black).Count;
        int dR = board.All(Piece.Rook, PieceColor.White).Count - board.All(Piece.Rook, PieceColor.Black).Count;
        int dP = board.All(Piece.Pawn, PieceColor.White).Count - board.All(Piece.Pawn, PieceColor.Black).Count;
        
        // Then, we must apply weights and return.
        return QUEEN * dQ + ROOK * dR + BISHOP_KNIGHT * (dB + dN) + PAWN * dP;
    }

}