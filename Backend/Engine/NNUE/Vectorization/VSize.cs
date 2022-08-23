using System.Numerics;

namespace Backend.Engine.NNUE.Vectorization;

public static class VSize
{

    public static readonly int Int = Vector<int>.Count;
    public static readonly int Short = Vector<short>.Count;

}