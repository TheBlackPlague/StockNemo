using System.Runtime.CompilerServices;
using Backend.Data.Struct;

namespace Backend.Data;

public class MoveSearchStack
{

    private const int SIZE = 128;

    private readonly MoveSearchStackItem[] Internal = new MoveSearchStackItem[SIZE];

    public ref MoveSearchStackItem this[int ply]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Internal.AA(ply);
    }

}