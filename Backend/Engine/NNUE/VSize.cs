using System.Numerics;

namespace Backend.Engine.NNUE;

public static class VSize
{

    public static readonly int Double = Vector<double>.Count;
    public static readonly int Int = Vector<int>.Count;
    public static readonly int Short = Vector<short>.Count;
    public static readonly int SByte = Vector<sbyte>.Count;

}