using System.Runtime.CompilerServices;
using Backend.Data.Struct;

namespace Backend.Data;

public class PrincipleVariationTable
{

    private const int SIZE = 64;

    private readonly int[] Length = new int[SIZE];
    private readonly OrderedMoveEntry[] Internal = new OrderedMoveEntry[SIZE * SIZE];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InitializeLength(int ply) => Length.AA(ply) = ply;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int ply, ref OrderedMoveEntry move) => Internal.AA(ply * SIZE + ply) = move;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Copy(int currentPly, int nextPly) =>
        Internal.AA(currentPly * SIZE + nextPly) = Internal.AA((currentPly + 1) * SIZE + nextPly);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool PlyInitialized(int currentPly, int nextPly) => nextPly < Length.AA(currentPly + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateLength(int ply) => Length.AA(ply) = Length.AA(ply + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Count() => Length.AA(0);

    public ref OrderedMoveEntry Get(int plyIndex) => ref Internal.AA(plyIndex);

}