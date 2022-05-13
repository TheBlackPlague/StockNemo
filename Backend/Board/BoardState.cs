namespace Backend.Board
{

    public struct BoardState
    {

        public bool WhiteTurn;
        
        public bool WhiteKCastle;
        public bool WhiteQCastle;
        public bool BlackKCastle;
        public bool BlackQCastle;
        
        public BitBoard EnPassantTarget;

    }

}