using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Backend.Engine.NNUE.Vectorization;

public static class VMethod
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<T> NewVector<T>(this T[] values, int index = 0) where T : struct
    {
        return Unsafe.ReadUnaligned<Vector<T>>(ref Unsafe.As<T, byte>(ref values.AA(index)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ToArray<T>(this Vector<T> vector, T[] array, int offset = 0) where T : struct
    {
        Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref array.AA(offset)), vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<T> Clamp<T>(this Vector<T> value, ref Vector<T> min, ref Vector<T> max) where T : struct
    {
        return Vector.Max(min, Vector.Min(max, value));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<int> MultiplyAddAdjacent(Vector<short> a, Vector<short> b)
    {
        if (Avx.IsSupported) {
            // ReSharper disable once InvertIf
            if (Avx2.IsSupported) {
                Vector256<short> one = a.AsVector256();
                Vector256<short> two = b.AsVector256();
                
                return Avx2.MultiplyAddAdjacent(one, two).AsVector();
            }
            
            return SoftwareFallback();
        }
        
        // ReSharper disable once InvertIf
        if (Sse2.IsSupported) {
            Vector128<short> one = a.AsVector128();
            Vector128<short> two = b.AsVector128();

            return Sse2.MultiplyAddAdjacent(one, two).AsVector();
        }

        return SoftwareFallback();

        Vector<int> SoftwareFallback()
        {
            Vector<short> c = a * b;
            Span<int> buffer = stackalloc int[VSize.Int];
            
            int vectorIndex = 0;
            for (int i = 0; i < VSize.Int; i++) {
                buffer[i] = c[vectorIndex] + c[vectorIndex + 1];
                vectorIndex += 2;
            }

            return new Vector<int>(buffer);
        }
    }

}