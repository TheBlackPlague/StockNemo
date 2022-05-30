using Backend;
using Backend.Data.Enum;

namespace Engine;

public static class Evaluation
{
    
    private const int QUEEN = 9;
    private const int ROOK = 5;
    private const int BISHOP_KNIGHT = 3;
    private const int PAWN = 1;

    public static int RelativeEvaluation(Board board)
    {
        return Material(board) * (board.WhiteTurn ? 1 : -1);
    }

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