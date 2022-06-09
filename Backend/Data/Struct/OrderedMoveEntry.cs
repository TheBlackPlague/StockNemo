using Backend.Data.Enum;

namespace Backend.Data.Struct;

public struct OrderedMoveEntry
{

    public readonly Square From = Square.Na;
    public readonly Square To = Square.Na;
    public readonly Promotion Promotion = Promotion.None;
    public int Score;

    public OrderedMoveEntry(Square from, Square to, Promotion promotion)
    {
        From = from;
        To = to;
        Promotion = promotion;
        Score = 0;
    }

}