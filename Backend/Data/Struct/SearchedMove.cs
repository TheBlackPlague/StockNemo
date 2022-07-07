using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data.Struct;

#pragma warning disable CS0660, CS0661
public readonly struct SearchedMove
#pragma warning restore CS0660, CS0661
{

    public static readonly SearchedMove Default = new(Square.Na, Square.Na, Promotion.None, 0);

    public readonly Square From;
    public readonly Square To;
    public readonly Promotion Promotion;
    public readonly int Evaluation;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(OrderedMoveEntry entry, SearchedMove searchedMove) => 
        entry.From == searchedMove.From && entry.To == searchedMove.To && entry.Promotion == searchedMove.Promotion;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(OrderedMoveEntry entry, SearchedMove searchedMove) => !(entry == searchedMove);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(SearchedMove first, SearchedMove second) =>
        first.From == second.From && first.To == second.To && first.Promotion == second.Promotion;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(SearchedMove first, SearchedMove second) => !(first == second);

    public SearchedMove(ref OrderedMoveEntry move, int evaluation)
    {
        From = move.From;
        To = move.To;
        Promotion = move.Promotion;
        Evaluation = evaluation;
    }

    private SearchedMove(Square from, Square to, Promotion promotion, int evaluation)
    {
        From = from;
        To = to;
        Promotion = promotion;
        Evaluation = evaluation;
    }

}