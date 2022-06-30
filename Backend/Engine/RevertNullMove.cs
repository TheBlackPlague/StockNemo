using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend.Engine;

public ref struct RevertNullMove
{

    #region Data

    public Square EnPassantTarget;

    #endregion
    
    #region BitBoardMap based Constructor

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RevertNullMove FromBitBoardMap(ref BitBoardMap map)
    {
        // Generate a RevertNullMove based on the current state of the map.
        return new RevertNullMove()
        {
            EnPassantTarget = map.EnPassantTarget
        };
    }

    #endregion

}