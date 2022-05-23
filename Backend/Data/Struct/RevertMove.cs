using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data.Struct
{

    public ref struct RevertMove
    {

        #region Data

        public bool WhiteKCastle;
        public bool WhiteQCastle;
        public bool BlackKCastle;
        public bool BlackQCastle;
        public Square EnPassantTarget;

        public bool WhiteTurn;

        public bool EnPassant;
        public Square From;
        public Square To;
        public Piece CapturedPiece;
        public PieceColor CapturedColor;

        public Square SecondaryFrom;
        public Square SecondaryTo;

        #endregion

        #region BitBoardMap based Constructor

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RevertMove FromBitBoardMap(ref BitBoardMap map)
        {
            return new RevertMove
            {
                WhiteKCastle = map.WhiteKCastle,
                WhiteQCastle = map.WhiteQCastle,
                BlackKCastle = map.BlackKCastle,
                BlackQCastle = map.BlackQCastle,
                EnPassantTarget = map.EnPassantTarget,
                WhiteTurn = map.WhiteTurn,
                SecondaryFrom = Square.Na,
                SecondaryTo = Square.Na,
                CapturedPiece = Piece.Empty,
                CapturedColor = PieceColor.None
            };
        }

        #endregion

    }

}