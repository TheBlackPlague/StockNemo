using System;
using System.Runtime.CompilerServices;
using Backend.Engine.NNUE.Vectorization;

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
        int size = A.Length * Unsafe.SizeOf<T>();
            
        Unsafe.CopyBlockUnaligned(
            ref Unsafe.As<T, byte>(ref target.A.AA(0)), 
            ref Unsafe.As<T, byte>(ref A.AA(0)), 
            (uint)size
        );
        Unsafe.CopyBlockUnaligned(
            ref Unsafe.As<T, byte>(ref target.B.AA(0)), 
            ref Unsafe.As<T, byte>(ref B.AA(0)), 
            (uint)size
        );
    }

}