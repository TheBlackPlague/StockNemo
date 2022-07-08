using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data.Struct;

#pragma warning disable CS0660, CS0661
public struct OrderedMoveEntry
#pragma warning restore CS0660, CS0661
{

    public static readonly OrderedMoveEntry Default = new(Square.Na, Square.Na, Promotion.None);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(OrderedMoveEntry first, OrderedMoveEntry second) =>
        first.From == second.From && first.To == second.To && first.Promotion == second.Promotion;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(OrderedMoveEntry first, OrderedMoveEntry second) => !(first == second);

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