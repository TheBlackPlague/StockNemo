using System.Runtime.CompilerServices;
using Backend.Data;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend.Engine;

public static class Evaluation
{
    
    // ReSharper disable once InconsistentNaming
    public static readonly MaterialDevelopmentTable MDT = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RelativeEvaluation(Board board)
    {
        return NormalEvaluation(board) * (-2 * (int)board.ColorToMove + 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NormalEvaluation(Board board)
    {
        int phase = 0;
        
        phase += board.All(Piece.Knight, PieceColor.White).Count + board.All(Piece.Knight, PieceColor.Black).Count;
        phase += board.All(Piece.Bishop, PieceColor.White).Count + board.All(Piece.Bishop, PieceColor.Black).Count;
        phase += (board.All(Piece.Rook, PieceColor.White).Count + board.All(Piece.Rook, PieceColor.Black).Count) * 2;
        phase += (board.All(Piece.Queen, PieceColor.White).Count + board.All(Piece.Queen, PieceColor.Black).Count) * 4;

        phase = 24 - phase;
        phase = (phase * 256 + 24 / 2) / 24;

        return (board.MaterialDevelopmentEvaluationEarly * (256 - phase) + 
                board.MaterialDevelopmentEvaluationLate * phase) / 256;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int InitialMaterialDevelopmentEvaluation(ref BitBoardMap map, Phase phase)
    {
        int whiteScore = 0;
        int blackScore = 0;

        #region White Score Calculation

        Piece piece = Piece.Pawn;
        while (piece != Piece.Empty) {
            BitBoardIterator pieceIterator = map[piece, PieceColor.White].GetEnumerator();
            Square pieceSq = pieceIterator.Current;
            while (pieceIterator.MoveNext()) {
                whiteScore += MDT[piece, (Square)((byte)pieceSq ^ 56), phase];
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
                blackScore += MDT[piece, pieceSq, phase];
                pieceSq = pieceIterator.Current;
            }
            piece++;
        }

        #endregion

        return whiteScore - blackScore;
    }

}