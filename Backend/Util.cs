using System.IO;
using Backend.Data.Enum;

namespace Backend
{

    public static class Util
    {

        public static (Piece, PieceColor) EmptyPieceState()
        {
            return (Piece.Empty, PieceColor.None);
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
            int rank = int.Parse(chsStr[1].ToString()) - 1;
            if (chsStr[0] > 72 || chsStr[0] < 65 || rank is < 0 or > 7) 
                throw new InvalidDataException("Cannot convert position " + chsStr.ToLower() + " to tuple.");
            return (chsStr[0] - 65, int.Parse(chsStr[1].ToString()) - 1);
        }

    }

}