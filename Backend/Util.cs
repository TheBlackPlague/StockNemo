using Backend.Board;

namespace Backend
{

    public static class Util
    {

        public static (Piece, PieceColor, MovedState) EmptyPieceState()
        {
            return (Piece.Empty, PieceColor.None, MovedState.Unmoved);
        }

        public static PieceColor OppositeColor(PieceColor color)
        {
            return color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        }

        public static bool AreOppositeColor(PieceColor one, PieceColor two)
        {
            return OppositeColor(one) == two;
        }

        public static string TupleToChessString((int, int) loc)
        {
            return ((char)(loc.Item1 + 65)).ToString() + (loc.Item2 + 1);
        }

        public static (int, int) ChessStringToTuple(string chsStr)
        {
            return (chsStr[0] - 65, int.Parse(chsStr[1].ToString()) - 1);
        }

    }

}