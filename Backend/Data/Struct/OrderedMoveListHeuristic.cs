namespace Backend.Data.Struct;

public ref struct OrderedMoveListHeuristic
{

    public readonly OrderedMoveEntry KillerMoveOne;
    public readonly OrderedMoveEntry KillerMoveTwo;

    public OrderedMoveListHeuristic(KillerMoveTable table, int ply)
    {
        KillerMoveOne = table[0, ply];
        KillerMoveTwo = table[1, ply];
    }

}