using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data.Struct;

public ref struct RevertMove
{

    #region Data

    public byte WhiteKCastle;
    public byte WhiteQCastle;
    public byte BlackKCastle;
    public byte BlackQCastle;
    public Square EnPassantTarget;

    public PieceColor ColorToMove;

    public bool Promotion;
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
        // Generate a RevertMove based on the current state of the map.
        return new RevertMove
        {
            WhiteKCastle = map.WhiteKCastle,
            WhiteQCastle = map.WhiteQCastle,
            BlackKCastle = map.BlackKCastle,
            BlackQCastle = map.BlackQCastle,
            EnPassantTarget = map.EnPassantTarget,
            ColorToMove = map.ColorToMove,
            Promotion = false,
            SecondaryFrom = Square.Na,
            SecondaryTo = Square.Na,
            CapturedPiece = Piece.Empty,
            CapturedColor = PieceColor.None
        };
    }

    #endregion

}