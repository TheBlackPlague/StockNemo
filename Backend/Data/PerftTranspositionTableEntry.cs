using System.Runtime.CompilerServices;

namespace Backend.Data;

public class PerftTranspositionTableEntry
{

    public ulong ZobristHash { get; private set; }
    public bool Set { get; private set; }
    
    private readonly ulong[] DepthCount = new ulong[8];
    private readonly bool[] DepthSet = new bool[8];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetZobristHash(ulong zobristHash)
    {
        ZobristHash = zobristHash;
        Set = true;
    }

    public ulong this[int depth]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            lock (this) {
                return DepthCount[depth - 1];
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            lock (this) {
                DepthSet[depth - 1] = true;
                DepthCount[depth - 1] = value;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool VerifyDepthSet(int depth)
    {
        lock (this) {
            return DepthSet[depth - 1];
        }
    }

}