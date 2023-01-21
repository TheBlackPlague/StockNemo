using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Backend.Engine.NNUE.Architecture.Basic;

[Serializable]
public class BasicAccumulator<T> where T : struct
{

    public readonly T[] White;
    public readonly T[] Black;

    public BasicAccumulator(int size)
    {
        White = new T[size];
        Black = new T[size];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(BasicAccumulator<T> target)
    {
        ref T a = ref MemoryMarshal.GetArrayDataReference(White);
        ref T b = ref MemoryMarshal.GetArrayDataReference(Black);
        ref T targetA = ref MemoryMarshal.GetArrayDataReference(target.White);
        ref T targetB = ref MemoryMarshal.GetArrayDataReference(target.Black);
        
        int size = White.Length * Unsafe.SizeOf<T>();
            
        Unsafe.CopyBlockUnaligned(
            ref Unsafe.As<T, byte>(ref targetA), 
            ref Unsafe.As<T, byte>(ref a), 
            (uint)size
        );
        Unsafe.CopyBlockUnaligned(
            ref Unsafe.As<T, byte>(ref targetB), 
            ref Unsafe.As<T, byte>(ref b), 
            (uint)size
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PreLoadBias(T[] bias)
    {
        ref T white = ref MemoryMarshal.GetArrayDataReference(White);
        ref T black = ref MemoryMarshal.GetArrayDataReference(Black);
        ref T biasRef = ref MemoryMarshal.GetArrayDataReference(bias);

        int size = White.Length * Unsafe.SizeOf<T>();
        
        Unsafe.CopyBlockUnaligned(
            ref Unsafe.As<T, byte>(ref white), 
            ref Unsafe.As<T, byte>(ref biasRef), 
            (uint)size
        );
        Unsafe.CopyBlockUnaligned(
            ref Unsafe.As<T, byte>(ref black), 
            ref Unsafe.As<T, byte>(ref biasRef), 
            (uint)size
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Zero()
    {
        Array.Clear(White);
        Array.Clear(Black);
    }

}