using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Engine.Data.Struct;

#pragma warning disable CS0660, CS0661
public readonly struct SearchedMove
#pragma warning restore CS0660, CS0661
{

    public readonly Square From;
    public readonly Square To;
    public readonly Promotion Promotion;
    public readonly int Evaluation;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(OrderedMoveEntry entry, SearchedMove searchedMove) => 
        entry.From == searchedMove.From && entry.To == searchedMove.To && entry.Promotion == searchedMove.Promotion;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(OrderedMoveEntry entry, SearchedMove searchedMove) => !(entry == searchedMove);

    public SearchedMove(ref OrderedMoveEntry move, int evaluation)
    {
        From = move.From;
        To = move.To;
        Promotion = move.Promotion;
        Evaluation = evaluation;
    }

    public SearchedMove(Square from, Square to, Promotion promotion, int evaluation)
    {
        From = from;
        To = to;
        Promotion = promotion;
        Evaluation = evaluation;
    }

}