using System.Numerics;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Engine.NNUE.Vectorization;

namespace Backend.Engine.NNUE;

public static class NN
{

    #region PieceToNN(Piece piece)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece PieceToNN(Piece piece)
    {
        return piece switch
        {
            Piece.Rook => piece + 2,
            Piece.Knight or Piece.Bishop => piece - 1,
            _ => piece
        };
    }

    #endregion

    #region Forward(value[] input, value[] weight, value[] output, int offset = 0)

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
    public static void Forward(short[] input, short[] weight, int[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int loopSize = inputSize / VSize.Short;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            int sum = 0;
            int vectorIndex = 0;
            for (int j = 0; j < loopSize; j++) {
                Vector<short> iVec = new(input, vectorIndex);
                Vector<short> wVec = new(weight, weightStride + vectorIndex);
                sum += Vector.Sum(Intrinsic.Multiply(iVec, wVec));
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

    #region AddToAll(value[] input, value[] delta, int offset)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToAll(double[] input, double[] delta, int offset)
    {
        int size = input.Length;
        int loopSize = size / VSize.Double;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<double> iVec = new(input, vectorIndex);
            Vector<double> dVec = new(delta, offset + vectorIndex);
            Vector<double> rVec = iVec + dVec;
            rVec.CopyTo(input, vectorIndex);
            vectorIndex += VSize.Double;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToAll(int[] input, int[] delta, int offset)
    {
        int size = input.Length;
        int loopSize = size / VSize.Int;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<int> iVec = new(input, vectorIndex);
            Vector<int> dVec = new(delta, offset + vectorIndex);
            Vector<int> rVec = iVec + dVec;
            rVec.CopyTo(input, vectorIndex);
            vectorIndex += VSize.Int;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToAll(short[] input, short[] delta, int offset)
    {
        int size = input.Length;
        int loopSize = size / VSize.Short;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<short> iVec = new(input, vectorIndex);
            Vector<short> dVec = new(delta, offset + vectorIndex);
            Vector<short> rVec = iVec + dVec;
            rVec.CopyTo(input, vectorIndex);
            vectorIndex += VSize.Short;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToAll(sbyte[] input, sbyte[] delta, int offset)
    {
        int size = input.Length;
        int loopSize = size / VSize.SByte;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<sbyte> iVec = new(input, vectorIndex);
            Vector<sbyte> dVec = new(delta, offset + vectorIndex);
            Vector<sbyte> rVec = iVec + dVec;
            rVec.CopyTo(input, vectorIndex);
            vectorIndex += VSize.SByte;
        }
    }
    
    #endregion
    
    #region SubtractFromAll(value[] input, value[] delta, int offset)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SubtractFromAll(double[] input, double[] delta, int offset)
    {
        int size = input.Length;
        int loopSize = size / VSize.Double;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<double> iVec = new(input, vectorIndex);
            Vector<double> dVec = new(delta, offset + vectorIndex);
            Vector<double> rVec = iVec - dVec;
            rVec.CopyTo(input, vectorIndex);
            vectorIndex += VSize.Double;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SubtractFromAll(int[] input, int[] delta, int offset)
    {
        int size = input.Length;
        int loopSize = size / VSize.Int;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<int> iVec = new(input, vectorIndex);
            Vector<int> dVec = new(delta, offset + vectorIndex);
            Vector<int> rVec = iVec - dVec;
            rVec.CopyTo(input, vectorIndex);
            vectorIndex += VSize.Int;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SubtractFromAll(short[] input, short[] delta, int offset)
    {
        int size = input.Length;
        int loopSize = size / VSize.Short;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<short> iVec = new(input, vectorIndex);
            Vector<short> dVec = new(delta, offset + vectorIndex);
            Vector<short> rVec = iVec - dVec;
            rVec.CopyTo(input, vectorIndex);
            vectorIndex += VSize.Short;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SubtractFromAll(sbyte[] input, sbyte[] delta, int offset)
    {
        int size = input.Length;
        int loopSize = size / VSize.SByte;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            Vector<sbyte> iVec = new(input, vectorIndex);
            Vector<sbyte> dVec = new(delta, offset + vectorIndex);
            Vector<sbyte> rVec = iVec - dVec;
            rVec.CopyTo(input, vectorIndex);
            vectorIndex += VSize.SByte;
        }
    }

    #endregion

}