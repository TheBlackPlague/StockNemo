using Backend.Data.Enum;

namespace Engine.Struct;

public readonly struct SearchedMove
{

    public readonly Square From;
    public readonly Square To;
    public readonly Promotion Promotion;
    public readonly int Score;

    public SearchedMove(ref OrderedMoveEntry move, int score)
    {
        From = move.From;
        To = move.To;
        Promotion = move.Promotion;
        Score = score;
    }

    public SearchedMove(Square from, Square to, Promotion promotion, int score)
    {
        From = from;
        To = to;
        Promotion = promotion;
        Score = score;
    }

}