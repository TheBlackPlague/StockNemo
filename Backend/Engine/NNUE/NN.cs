using System.Numerics;
using System.Runtime.CompilerServices;

namespace Backend.Engine.NNUE;

public static class NN
{

    #region void Forward(value[] input, value[] weight, value[] output, int offset = 0)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forward(double[] input, double[] weight, double[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int loopSize = inputSize / VSize.Double;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            double sum = 0;
            int vectorIndex = 0;
            for (int j = 0; j < loopSize; j++) {
                Vector<double> iVec = new(input, vectorIndex);
                Vector<double> wVec = new(weight, weightStride + vectorIndex);
                sum += Vector.Sum(iVec * wVec);
                vectorIndex += VSize.Double;
            }
            output.AA(offset + i) = sum;
            weightStride += inputSize;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forward(int[] input, int[] weight, int[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int loopSize = inputSize / VSize.Int;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            int sum = 0;
            int vectorIndex = 0;
            for (int j = 0; j < loopSize; j++) {
                Vector<int> iVec = new(input, vectorIndex);
                Vector<int> wVec = new(weight, weightStride + vectorIndex);
                sum += Vector.Sum(iVec * wVec);
                vectorIndex += VSize.Int;
            }
            output.AA(offset + i) = sum;
            weightStride += inputSize;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forward(short[] input, short[] weight, short[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int loopSize = inputSize / VSize.Short;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            short sum = 0;
            int vectorIndex = 0;
            for (int j = 0; j < loopSize; j++) {
                Vector<short> iVec = new(input, vectorIndex);
                Vector<short> wVec = new(weight, weightStride + vectorIndex);
                sum += Vector.Sum(iVec * wVec);
                vectorIndex += VSize.Short;
            }
            output.AA(offset + i) = sum;
            weightStride += inputSize;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forward(sbyte[] input, sbyte[] weight, sbyte[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int loopSize = inputSize / VSize.SByte;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            sbyte sum = 0;
            int vectorIndex = 0;
            for (int j = 0; j < loopSize; j++) {
                Vector<sbyte> iVec = new(input, vectorIndex);
                Vector<sbyte> wVec = new(weight, weightStride + vectorIndex);
                sum += Vector.Sum(iVec * wVec);
                vectorIndex += VSize.SByte;
            }
            output.AA(offset + i) = sum;
            weightStride += inputSize;
        }
    }
    
    #endregion

    #region ClippedReLU(value[] input, value[] bias, value[] output, value min, value max, int offset = 0)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClippedReLU(double[] input, double[] bias, double[] output, double min, double max,
        int offset = 0)
    {
        int size = input.Length;
        int loopSize = size / VSize.Double;
        Vector<double> minVec = new(min);
        Vector<double> maxVec = new(max);

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<double> iVec = new(input, vectorIndex);
            Vector<double> bVec = new(bias, vectorIndex);
            Vector<double> clamped = (iVec + bVec).Clamp(ref minVec, ref maxVec);
            clamped.CopyTo(output, offset + vectorIndex);
            vectorIndex += VSize.Double;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClippedReLU(int[] input, int[] bias, int[] output, int min, int max, int offset = 0)
    {
        int size = input.Length;
        int loopSize = size / VSize.Int;
        Vector<int> minVec = new(min);
        Vector<int> maxVec = new(max);
        
        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<int> iVec = new(input, vectorIndex);
            Vector<int> bVec = new(bias, vectorIndex);
            Vector<int> clamped = (iVec + bVec).Clamp(ref minVec, ref maxVec);
            clamped.CopyTo(output, offset + vectorIndex);
            vectorIndex += VSize.Int;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClippedReLU(short[] input, short[] bias, short[] output, short min, short max, int offset = 0)
    {
        int size = input.Length;
        int loopSize = size / VSize.Short;
        Vector<short> minVec = new(min);
        Vector<short> maxVec = new(max);

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<short> iVec = new(input, vectorIndex);
            Vector<short> bVec = new(bias, vectorIndex);
            Vector<short> clamped = (iVec + bVec).Clamp(ref minVec, ref maxVec);
            clamped.CopyTo(output, offset + vectorIndex);
            vectorIndex += VSize.Short;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClippedReLU(sbyte[] input, sbyte[] bias, sbyte[] output, sbyte min, sbyte max, int offset = 0)
    {
        int size = input.Length;
        int loopSize = size / VSize.SByte;
        Vector<sbyte> minVec = new(min);
        Vector<sbyte> maxVec = new(max);

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<sbyte> iVec = new(input, vectorIndex);
            Vector<sbyte> bVec = new(bias, vectorIndex);
            Vector<sbyte> clamped = (iVec + bVec).Clamp(ref minVec, ref maxVec);
            clamped.CopyTo(output, offset + vectorIndex);
            vectorIndex += VSize.SByte;
        }
    }

    #endregion

    #region AddToAll(value[] all, value[] delta, int offset = 0)

    public static void AddToAll(double[] all, double[] delta, int offset)
    {
        int allSize = all.Length;
        int loopSize = allSize / VSize.Double;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<double> aVec = new(all, vectorIndex);
            Vector<double> dVec = new(delta, offset + vectorIndex);
            Vector<double> result = aVec + dVec;
            result.CopyTo(all, vectorIndex);
            vectorIndex += VSize.Double;
        }
    }
    
    public static void AddToAll(int[] all, int[] delta, int offset)
    {
        int allSize = all.Length;
        int loopSize = allSize / VSize.Int;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<int> aVec = new(all, vectorIndex);
            Vector<int> dVec = new(delta, offset + vectorIndex);
            Vector<int> result = aVec + dVec;
            result.CopyTo(all, vectorIndex);
            vectorIndex += VSize.Int;
        }
    }
    
    public static void AddToAll(short[] all, short[] delta, int offset)
    {
        int allSize = all.Length;
        int loopSize = allSize / VSize.Short;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<short> aVec = new(all, vectorIndex);
            Vector<short> dVec = new(delta, offset + vectorIndex);
            Vector<short> result = aVec + dVec;
            result.CopyTo(all, vectorIndex);
            vectorIndex += VSize.Short;
        }
    }
    
    public static void AddToAll(sbyte[] all, sbyte[] delta, int offset)
    {
        int allSize = all.Length;
        int loopSize = allSize / VSize.SByte;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<sbyte> aVec = new(all, vectorIndex);
            Vector<sbyte> dVec = new(delta, offset + vectorIndex);
            Vector<sbyte> result = aVec + dVec;
            result.CopyTo(all, vectorIndex);
            vectorIndex += VSize.SByte;
        }
    }

    #endregion
    
    #region SubtractFromAll(value[] all, value[] delta, int offset = 0)

    public static void SubtractFromAll(double[] all, double[] delta, int offset)
    {
        int allSize = all.Length;
        int loopSize = allSize / VSize.Double;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<double> aVec = new(all, vectorIndex);
            Vector<double> dVec = new(delta, offset + vectorIndex);
            Vector<double> result = aVec - dVec;
            result.CopyTo(all, vectorIndex);
            vectorIndex += VSize.Double;
        }
    }
    
    public static void SubtractFromAll(int[] all, int[] delta, int offset)
    {
        int allSize = all.Length;
        int loopSize = allSize / VSize.Int;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<int> aVec = new(all, vectorIndex);
            Vector<int> dVec = new(delta, offset + vectorIndex);
            Vector<int> result = aVec - dVec;
            result.CopyTo(all, vectorIndex);
            vectorIndex += VSize.Int;
        }
    }
    
    public static void SubtractFromAll(short[] all, short[] delta, int offset)
    {
        int allSize = all.Length;
        int loopSize = allSize / VSize.Short;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<short> aVec = new(all, vectorIndex);
            Vector<short> dVec = new(delta, offset + vectorIndex);
            Vector<short> result = aVec - dVec;
            result.CopyTo(all, vectorIndex);
            vectorIndex += VSize.Short;
        }
    }
    
    public static void SubtractFromAll(sbyte[] all, sbyte[] delta, int offset)
    {
        int allSize = all.Length;
        int loopSize = allSize / VSize.SByte;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<sbyte> aVec = new(all, vectorIndex);
            Vector<sbyte> dVec = new(delta, offset + vectorIndex);
            Vector<sbyte> result = aVec - dVec;
            result.CopyTo(all, vectorIndex);
            vectorIndex += VSize.SByte;
        }
    }

    #endregion

}