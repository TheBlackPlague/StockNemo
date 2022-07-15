using Backend.Data.Enum;

namespace Backend.Data.Struct;

public struct EvaluationStack
{
    public int WhiteRooks;
    public int WhiteKnights;
    public int WhiteBishops;
    public int WhiteQueens;
    public int BlackRooks;
    public int BlackKnights;
    public int BlackBishops;
    public int BlackQueens;

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