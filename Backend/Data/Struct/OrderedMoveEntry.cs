﻿using Backend.Data.Enum;

namespace Backend.Data.Struct;

public struct OrderedMoveEntry
{

    public Square From;
    public Square To;
    public Promotion Promotion;

    public OrderedMoveEntry(Square from, Square to, Promotion promotion)
    {
        From = from;
        To = to;
        Promotion = promotion;
    }

}