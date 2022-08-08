using System;
using System.Runtime.CompilerServices;

namespace Backend.Engine.NNUE;

public static class NN
{

    #region void Forward(value[] input, value[] weight, value[] output, int offset = 0)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forward(double[] input, double[] weight, double[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            double sum = 0;
            for (int j = 0; j < inputSize; j++) sum += input.AA(j) * weight.AA(weightStride + j);
            output.AA(offset + i) = sum;
            weightStride += inputSize;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forward(int[] input, int[] weight, int[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            int sum = 0;
            for (int j = 0; j < inputSize; j++) sum += input.AA(j) * weight.AA(weightStride + j);
            output.AA(offset + i) = sum;
            weightStride += inputSize;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forward(short[] input, short[] weight, short[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            short sum = 0;
            for (int j = 0; j < inputSize; j++) sum += (short)(input.AA(j) * weight.AA(weightStride + j));
            output.AA(offset + i) = sum;
            weightStride += inputSize;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forward(sbyte[] input, sbyte[] weight, sbyte[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            sbyte sum = 0;
            for (int j = 0; j < inputSize; j++) sum += (sbyte)(input.AA(j) * weight.AA(weightStride + j));
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
        for (int i = 0; i < size; i++) output.AA(offset + i) = Math.Clamp(input.AA(i) + bias.AA(i), min, max);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClippedReLU(int[] input, int[] bias, int[] output, int min, int max, int offset = 0)
    {
        int size = input.Length;
        for (int i = 0; i < size; i++) output.AA(offset + i) = Math.Clamp(input.AA(i) + bias.AA(i), min, max);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClippedReLU(short[] input, short[] bias, short[] output, short min, short max, int offset = 0)
    {
        int size = input.Length;
        for (int i = 0; i < size; i++) 
            output.AA(offset + i) = Math.Clamp((short)(input.AA(i) + bias.AA(i)), min, max);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClippedReLU(sbyte[] input, sbyte[] bias, sbyte[] output, sbyte min, sbyte max, int offset = 0)
    {
        int size = input.Length;
        for (int i = 0; i < size; i++) 
            output.AA(offset + i) = Math.Clamp((sbyte)(input.AA(i) + bias.AA(i)), min, max);
    }

    #endregion

}