using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Backend.Engine.NNUE;

public static class Intrinsic
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<int> Multiply(Vector<short> a, Vector<short> b)
    {
        if (Avx2.X64.IsSupported) {
            Vector256<short> one = a.AsVector256();
            Vector256<short> two = b.AsVector256();

            return Avx2.MultiplyAddAdjacent(one, two).AsVector();
        }
        
        // ReSharper disable once InvertIf
        if (Sse2.X64.IsSupported) {
            Vector128<short> one = a.AsVector128();
            Vector128<short> two = b.AsVector128();

            return Sse2.MultiplyAddAdjacent(one, two).AsVector();
        }

        return SoftwareFallback();

        Vector<int> SoftwareFallback()
        {
            Span<int> buffer = stackalloc int[VSize.Int];
            int vectorIndex = 0;
            for (int i = 0; i < VSize.Int; i++) {
                buffer[i] = a[vectorIndex] * b[vectorIndex] + a[vectorIndex + 1] * b[vectorIndex + 1];
                vectorIndex += 2;
            }
            return new Vector<int>(buffer);
        }
    } 

}