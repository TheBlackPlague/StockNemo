using Backend.Board;

namespace Backend.Move
{

    public class MoveState
    {

        public (int, int) From;
        public (int, int) To;

        public (Piece, PieceColor, (int, int))? Captured;

    }

}