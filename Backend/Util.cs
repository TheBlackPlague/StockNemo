using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
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
    public static byte ToByte(this bool value) => Unsafe.As<bool, byte>(ref value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToUciNotation(this Promotion promotion)
    {
        string notation = promotion.ToString()[0].ToString().ToLower();
        return promotion == Promotion.Knight ? "n" : notation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceColor OppositeColor(PieceColor color) => (PieceColor)((int)color ^ 0x1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // ReSharper disable once InconsistentNaming
    public static ref T AA<T>(this T[] array, int index)
    {
#if DEBUG
        return ref array[index];
#else
        return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // ReSharper disable once InconsistentNaming
    public static ref T DJAA<T>(this T[][] array, int firstIndex, int secondIndex)
    {
#if DEBUG
        return ref array.AA(firstIndex)[secondIndex];
#else
        return 
            ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array.AA(firstIndex)), secondIndex);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void SaveBinary<T>(T data, Stream stream)
    {
        BinaryFormatter writer = new();
#pragma warning disable SYSLIB0011
        writer.Serialize(stream, data);
#pragma warning restore SYSLIB0011
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static T ReadBinary<T>(Stream stream)
    {
        BinaryFormatter reader = new();
#pragma warning disable SYSLIB0011
        return (T)reader.Deserialize(stream);
#pragma warning restore SYSLIB0011
    }

}