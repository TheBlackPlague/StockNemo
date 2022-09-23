using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Backend.Data;
using Backend.Data.Enum;
using Backend.Data.Struct;
using Backend.Engine.NNUE.Architecture.Basic;

namespace Backend.Engine;

public static class Evaluation
{

    private const string NNUE_FILE = "Backend.Engine.NNUE.Model.BasicNNUE";
    private const string HASH = "a740c1c9db";

    private const int BISHOP_PAIR_EARLY = 25;
    private const int BISHOP_PAIR_LATE = 50;
    
    // ReSharper disable once InconsistentNaming
    public static readonly MaterialDevelopmentTable MDT = new();
    public static readonly BasicNNUE NNUE;

    static Evaluation()
    {
        // NNUE = new BasicNNUE();
        // NNUE.FromJson(File.OpenRead());
        // Util.SaveBinary(NNUE, File.OpenWrite());
        const string resource = NNUE_FILE + "-" + HASH + ".nnue";
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
        NNUE = Util.ReadBinary<BasicNNUE>(stream);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RelativeEvaluation(Board board)
    {
        return NNEvaluation(board);
        // return NormalEvaluation(board) * (-2 * (int)board.ColorToMove + 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int NNEvaluation(Board board)
    {
        // NNUE.RefreshAccumulator(board);
        return NNUE.Evaluate(board.ColorToMove);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NormalEvaluation(Board board)
    {
        int earlyGameEvaluation = board.MaterialDevelopmentEvaluationEarly;
        int lateGameEvaluation = board.MaterialDevelopmentEvaluationLate;
        
        int whiteBishopCount = board.All(Piece.Bishop, PieceColor.White).Count;
        int blackBishopCount = board.All(Piece.Bishop, PieceColor.Black).Count;

        #region Bishop Pair Evaluation

        // Give bonus to the side which has a bishop pair while the opposing side doesn't.
        earlyGameEvaluation += ((whiteBishopCount >> 1) - (blackBishopCount >> 1)) * BISHOP_PAIR_EARLY;
        lateGameEvaluation += ((whiteBishopCount >> 1) - (blackBishopCount >> 1)) * BISHOP_PAIR_LATE;

        #endregion

        #region Phase Calculation

        // Calculate the phase of the game.
        // Currently supported solid phases:
        // - Early
        // - Late
        
        int phase = 0;
        phase += board.All(Piece.Knight, PieceColor.White).Count + board.All(Piece.Knight, PieceColor.Black).Count;
        phase += whiteBishopCount + blackBishopCount;
        phase += (board.All(Piece.Rook, PieceColor.White).Count + board.All(Piece.Rook, PieceColor.Black).Count) * 2;
        phase += (board.All(Piece.Queen, PieceColor.White).Count + board.All(Piece.Queen, PieceColor.Black).Count) * 4;

        phase = 24 - phase;
        phase = (phase * 256 + 24 / 2) / 24;

        #endregion

        // Return final evaluation based on interpolation between phases.
        return (earlyGameEvaluation * (256 - phase) + lateGameEvaluation * phase) / 256;
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