using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Backend.Data.Enum;

namespace Backend;

public static class Util
{

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void RunStaticConstructor()
    {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()) 
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceColor OppositeColor(PieceColor color) => (PieceColor)((int)color ^ 0x1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // ReSharper disable once InconsistentNaming
    public static ref T AA<T>(this T[] array, int index)
    {
        return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // ReSharper disable once InconsistentNaming
    public static ref T DJAA<T>(this T[][] array, int firstIndex, int secondIndex)
    {
        return 
            ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array.AA(firstIndex)), secondIndex);
    }

}