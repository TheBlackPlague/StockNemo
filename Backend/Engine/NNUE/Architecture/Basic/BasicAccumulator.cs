using System;

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

}