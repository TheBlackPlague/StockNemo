using System;

namespace Backend.Data.Struct;

public ref struct OrderedMoveList
{

    public int Count { get; private set; }
    
    private readonly Span<OrderedMoveEntry> Internal;

    public OrderedMoveList(Span<OrderedMoveEntry> @internal)
    {
        Internal = @internal;
        Count = 0;
    }

    public OrderedMoveEntry this[int i]
    {
        get => Internal[i];
        set
        {
            Internal[i] = value;
            Count++;
        }
    }

}