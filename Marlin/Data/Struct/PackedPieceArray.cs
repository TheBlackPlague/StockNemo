using Backend.Data.Enum;

namespace Marlin.Data.Struct;

public struct PackedPieceArray
{

    public ulong Lower;
    public ulong Upper;

}

public struct PackedPieceArrayGenerator
{

    public PackedPieceArray Internal = default;
    private int Shift = 0;
    private bool Upper = false;

    public PackedPieceArrayGenerator() {}

    public void AddPiece(Piece piece, PieceColor color)
    {
        if (Upper) {
            Internal.Upper |= ((ulong)color << 3 | (ulong)piece) << Shift;
            Shift += 4;
        } else {
            Internal.Lower |= ((ulong)color << 3 | (ulong)piece) << Shift;
            Shift += 4;

            if (Shift != 64) return;
            
            Shift = 0;
            Upper = true;
        }
    }

}