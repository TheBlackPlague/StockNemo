using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Backend.Data;

[StructLayout(LayoutKind.Sequential)]
public class PerftTranspositionTableEntry
{

    public ulong ZobristHash { get; private set; }
    public bool Set { get; private set; }
    
    private readonly ulong[] DepthCount = new ulong[9];
    private readonly bool[] DepthSet = new bool[9];

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
                return DepthCount.AA(depth - 1);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            lock (this) {
                DepthSet.AA(depth - 1) = true;
                DepthCount.AA(depth - 1) = value;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool VerifyDepthSet(int depth)
    {
        lock (this) {
            return DepthSet.AA(depth - 1);
        }
    }

}