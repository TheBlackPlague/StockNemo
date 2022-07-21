using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Backend.Data.Enum;

namespace Backend.Data.Struct;

#pragma warning disable CS0660, CS0661
[StructLayout(LayoutKind.Explicit)]
public struct OrderedMoveEntry
#pragma warning restore CS0660, CS0661
{

    public static readonly OrderedMoveEntry Default = new(Square.Na, Square.Na, Promotion.None);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(OrderedMoveEntry first, OrderedMoveEntry second) =>
        first.MovePacked == second.MovePacked;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(OrderedMoveEntry first, OrderedMoveEntry second) => !(first == second);

    [FieldOffset(4)] public readonly Square From = Square.Na;
    [FieldOffset(5)] public readonly Square To = Square.Na;
    [FieldOffset(6)] public readonly Promotion Promotion = Promotion.None;
    
    [FieldOffset(0)] public int Score;
    [FieldOffset(4)] private readonly int MovePacked;

    public OrderedMoveEntry(Square from, Square to, Promotion promotion)
    {
        MovePacked = 0;
        From = from;
        To = to;
        Promotion = promotion;
        Score = 0;
    }

}