using Backend.Data.Enum;

namespace Backend.Data.Struct;

public struct EvaluationStack
{
    public static int WhiteRooks;
    public static int WhiteKnights;
    public static int WhiteBishops;
    public static int WhiteQueens;
    public static int BlackRooks;
    public static int BlackKnights;
    public static int BlackBishops;
    public static int BlackQueens;

    public EvaluationStack(Board board)
    {
        WhiteRooks = board.All(Piece.Rook, PieceColor.White).Count;
        WhiteKnights = board.All(Piece.Knight, PieceColor.White).Count;
        WhiteBishops = board.All(Piece.Bishop, PieceColor.White).Count;
        WhiteQueens = board.All(Piece.Queen, PieceColor.White).Count;
        BlackRooks = board.All(Piece.Rook, PieceColor.Black).Count;
        BlackKnights = board.All(Piece.Knight, PieceColor.Black).Count;
        BlackBishops = board.All(Piece.Bishop, PieceColor.Black).Count;
        BlackQueens = board.All(Piece.Queen, PieceColor.Black).Count;
    }
}