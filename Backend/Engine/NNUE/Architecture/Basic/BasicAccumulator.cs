using System;
using System.Runtime.CompilerServices;

namespace Backend.Engine.NNUE.Architecture.Basic;

[Serializable]
public class BasicAccumulator<T> where T : struct
{

    public readonly T[] A;
    public readonly T[] B;

    public BasicAccumulator(int size)
    {
        A = new T[size];
        B = new T[size];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(BasicAccumulator<T> target)
    {
        Buffer.BlockCopy(A, 0, target.A, 0, A.Length * Unsafe.SizeOf<T>());
        Buffer.BlockCopy(B, 0, target.B, 0, B.Length * Unsafe.SizeOf<T>());
    }

}