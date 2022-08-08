using System;

namespace Backend.Engine.NNUE.Architecture.Basic;

[Serializable]
public class BasicAccumulator
{

    public readonly double[] A;
    public readonly double[] B;

    public BasicAccumulator(int size)
    {
        A = new double[size];
        B = new double[size];
    }

}