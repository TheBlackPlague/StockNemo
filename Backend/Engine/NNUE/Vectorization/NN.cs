using System.Numerics;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Engine.NNUE.Vectorization;

public static class NN
{

    private const int UNROLL = 4;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forward(short[] input, short[] weight, short[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int loopSize = inputSize / VSize.Short / UNROLL;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            Vector<short> sum = Vector<short>.Zero;
            int vectorIndex = 0;
            for (int j = 0; j < loopSize; j++) {
                int unrolledIndex = vectorIndex + VSize.Short;
                int unrolledIndex2 = unrolledIndex + VSize.Short;
                int unrolledIndex3 = unrolledIndex2 + VSize.Short;
                
                Vector<short> iVec = input.LoadVector(vectorIndex);
                Vector<short> wVec = weight.LoadVector(weightStride + vectorIndex);
                sum += iVec * wVec;
                
                Vector<short> iVec2 = input.LoadVector(unrolledIndex);
                Vector<short> wVec2 = weight.LoadVector(weightStride + unrolledIndex);
                sum += iVec2 * wVec2;
                
                Vector<short> iVec3 = input.LoadVector(unrolledIndex2);
                Vector<short> wVec3 = weight.LoadVector(weightStride + unrolledIndex2);
                sum += iVec3 * wVec3;
                
                Vector<short> iVec4 = input.LoadVector(unrolledIndex3);
                Vector<short> wVec4 = weight.LoadVector(weightStride + unrolledIndex3);
                sum += iVec4 * wVec4;
                
                vectorIndex = unrolledIndex3 + VSize.Short;
            }
            output.AA(offset + i) = Vector.Sum(sum);
            weightStride += inputSize;
        }
    }
    
#if DEBUG

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forward(short[] input, short[] weight, int[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int loopSize = inputSize / VSize.Short / UNROLL;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            Vector<int> sum = Vector<int>.Zero;
            int vectorIndex = 0;
            for (int j = 0; j < loopSize; j++) {
                int unrolledIndex = vectorIndex + VSize.Short;
                int unrolledIndex2 = unrolledIndex + VSize.Short;
                int unrolledIndex3 = unrolledIndex2 + VSize.Short;
                
                Vector<short> iVec = input.NewVector(vectorIndex);
                Vector<short> wVec = weight.NewVector(weightStride + vectorIndex);
                sum += VMethod.MultiplyAddAdjacent(iVec, wVec);
                
                Vector<short> iVec2 = input.NewVector(unrolledIndex);
                Vector<short> wVec2 = weight.NewVector(weightStride + unrolledIndex);
                sum += VMethod.MultiplyAddAdjacent(iVec2, wVec2);
                
                Vector<short> iVec3 = input.NewVector(unrolledIndex2);
                Vector<short> wVec3 = weight.NewVector(weightStride + unrolledIndex2);
                sum += VMethod.MultiplyAddAdjacent(iVec3, wVec3);
                
                Vector<short> iVec4 = input.NewVector(unrolledIndex3);
                Vector<short> wVec4 = weight.NewVector(weightStride + unrolledIndex3);
                sum += VMethod.MultiplyAddAdjacent(iVec4, wVec4);
                
                vectorIndex = unrolledIndex3 + VSize.Short;
            }
            output.AA(offset + i) = Vector.Sum(sum);
            weightStride += inputSize;
        }
    }

#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClippedReLUFlattenAndForward(short[] inputA, short[] inputB, short[] bias, short[] weight, 
        int[] output, short min, short max, int separationIndex, int offset = 0)
    {
        int inputSize = inputA.Length + inputB.Length;
        int loopSize = inputSize / VSize.Short / UNROLL;
        int outputSize = output.Length;
        int weightStride = 0;
        
        Vector<short> minVec = new(min);
        Vector<short> maxVec = new(max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        short[] InputReference(int index) => index < separationIndex ? inputA : inputB;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int RelativeIndex(int index) => index < separationIndex ? index : index - separationIndex;

        for (int i = 0; i < outputSize; i++) {
            Vector<int> sum = Vector<int>.Zero;
            int vectorIndex = 0;
            for (int j = 0; j < loopSize; j++) {
                short[] input = InputReference(vectorIndex);

                int unrolledIndex = vectorIndex + VSize.Short;
                int unrolledIndex2 = unrolledIndex + VSize.Short;
                int unrolledIndex3 = unrolledIndex2 + VSize.Short;
                
                int rIndex = RelativeIndex(vectorIndex);
                int unrolledRIndex = rIndex + VSize.Short;
                int unrolledRIndex2 = unrolledRIndex + VSize.Short;
                int unrolledRIndex3 = unrolledRIndex2 + VSize.Short;

                Vector<short> iVec = input.LoadVector(rIndex);
                Vector<short> bVec = bias.LoadVector(rIndex);
                Vector<short> wVec = weight.LoadVector(weightStride + vectorIndex);
                Vector<short> clamped = (iVec + bVec).Clamp(ref minVec, ref maxVec);
                sum += VMethod.MultiplyAddAdjacent(clamped, wVec);
                
                Vector<short> iVec2 = input.LoadVector(unrolledRIndex);
                Vector<short> bVec2 = bias.LoadVector(unrolledRIndex);
                Vector<short> wVec2 = weight.LoadVector(weightStride + unrolledIndex);
                Vector<short> clamped2 = (iVec2 + bVec2).Clamp(ref minVec, ref maxVec);
                sum += VMethod.MultiplyAddAdjacent(clamped2, wVec2);
                
                Vector<short> iVec3 = input.LoadVector(unrolledRIndex2);
                Vector<short> bVec3 = bias.LoadVector(unrolledRIndex2);
                Vector<short> wVec3 = weight.LoadVector(weightStride + unrolledIndex2);
                Vector<short> clamped3 = (iVec3 + bVec3).Clamp(ref minVec, ref maxVec);
                sum += VMethod.MultiplyAddAdjacent(clamped3, wVec3);
                
                Vector<short> iVec4 = input.LoadVector(unrolledRIndex3);
                Vector<short> bVec4 = bias.LoadVector(unrolledRIndex3);
                Vector<short> wVec4 = weight.LoadVector(weightStride + unrolledIndex3);
                Vector<short> clamped4 = (iVec4 + bVec4).Clamp(ref minVec, ref maxVec);
                sum += VMethod.MultiplyAddAdjacent(clamped4, wVec4);
                
                vectorIndex = unrolledIndex3 + VSize.Short;
            }
            
            output.AA(offset + i) = Vector.Sum(sum);
            weightStride += inputSize;
        }
    }

#if DEBUG

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClippedReLU(short[] input, short[] bias, short[] output, short min, short max, int offset = 0)
    {
        int size = input.Length;
        int loopSize = size / VSize.Short / UNROLL;
        Vector<short> minVec = new(min);
        Vector<short> maxVec = new(max);

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            int unrolledIndex = vectorIndex + VSize.Short;
            int unrolledIndex2 = unrolledIndex + VSize.Short;
            int unrolledIndex3 = unrolledIndex2 + VSize.Short;

            Vector<short> iVec = input.NewVector(vectorIndex);
            Vector<short> bVec = bias.NewVector(vectorIndex);
            Vector<short> clamped = (iVec + bVec).Clamp(ref minVec, ref maxVec);
            clamped.ToArray(output, offset + vectorIndex);
            
            Vector<short> iVec2 = input.NewVector(unrolledIndex);
            Vector<short> bVec2 = bias.NewVector(unrolledIndex);
            Vector<short> clamped2 = (iVec2 + bVec2).Clamp(ref minVec, ref maxVec);
            clamped2.ToArray(output, offset + unrolledIndex);
            
            Vector<short> iVec3 = input.NewVector(unrolledIndex2);
            Vector<short> bVec3 = bias.NewVector(unrolledIndex2);
            Vector<short> clamped3 = (iVec3 + bVec3).Clamp(ref minVec, ref maxVec);
            clamped3.ToArray(output, offset + unrolledIndex2);
            
            Vector<short> iVec4 = input.NewVector(unrolledIndex3);
            Vector<short> bVec4 = bias.NewVector(unrolledIndex3);
            Vector<short> clamped4 = (iVec4 + bVec4).Clamp(ref minVec, ref maxVec);
            clamped4.ToArray(output, offset + unrolledIndex3);
            
            vectorIndex = unrolledIndex3 + VSize.Short;
        }
    }

#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToAll(short[] inputA, short[] inputB, short[] delta, int offsetA, int offsetB)
    {
        int size = inputA.Length;
        int loopSize = size / VSize.Short / UNROLL;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            int unrolledIndex = vectorIndex + VSize.Short;
            int unrolledIndex2 = unrolledIndex + VSize.Short;
            int unrolledIndex3 = unrolledIndex2 + VSize.Short;
            
            Vector<short> iAVec = inputA.LoadVector(vectorIndex);
            Vector<short> dAVec = delta.LoadVector(offsetA + vectorIndex);
            Vector<short> rAVec = iAVec + dAVec;
            rAVec.ToArray(inputA, vectorIndex);
            
            Vector<short> iAVec2 = inputA.LoadVector(unrolledIndex);
            Vector<short> dAVec2 = delta.LoadVector(offsetA + unrolledIndex);
            Vector<short> rAVec2 = iAVec2 + dAVec2;
            rAVec2.ToArray(inputA, unrolledIndex);
            
            Vector<short> iAVec3 = inputA.LoadVector(unrolledIndex2);
            Vector<short> dAVec3 = delta.LoadVector(offsetA + unrolledIndex2);
            Vector<short> rAVec3 = iAVec3 + dAVec3;
            rAVec3.ToArray(inputA, unrolledIndex2);
            
            Vector<short> iAVec4 = inputA.LoadVector(unrolledIndex3);
            Vector<short> dAVec4 = delta.LoadVector(offsetA + unrolledIndex3);
            Vector<short> rAVec4 = iAVec4 + dAVec4;
            rAVec4.ToArray(inputA, unrolledIndex3);
            
            Vector<short> iBVec = inputB.LoadVector(vectorIndex);
            Vector<short> dBVec = delta.LoadVector(offsetB + vectorIndex);
            Vector<short> rBVec = iBVec + dBVec;
            rBVec.ToArray(inputB, vectorIndex);
            
            Vector<short> iBVec2 = inputB.LoadVector(unrolledIndex);
            Vector<short> dBVec2 = delta.LoadVector(offsetB + unrolledIndex);
            Vector<short> rBVec2 = iBVec2 + dBVec2;
            rBVec2.ToArray(inputB, unrolledIndex);
            
            Vector<short> iBVec3 = inputB.LoadVector(unrolledIndex2);
            Vector<short> dBVec3 = delta.LoadVector(offsetB + unrolledIndex2);
            Vector<short> rBVec3 = iBVec3 + dBVec3;
            rBVec3.ToArray(inputB, unrolledIndex2);
            
            Vector<short> iBVec4 = inputB.LoadVector(unrolledIndex3);
            Vector<short> dBVec4 = delta.LoadVector(offsetB + unrolledIndex3);
            Vector<short> rBVec4 = iBVec4 + dBVec4;
            rBVec4.ToArray(inputB, unrolledIndex3);

            vectorIndex = unrolledIndex3 + VSize.Short;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SubtractFromAll(short[] inputA, short[] inputB, short[] delta, int offsetA, int offsetB)
    {
        int size = inputA.Length;
        int loopSize = size / VSize.Short / UNROLL;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            int unrolledIndex = vectorIndex + VSize.Short;
            int unrolledIndex2 = unrolledIndex + VSize.Short;
            int unrolledIndex3 = unrolledIndex2 + VSize.Short;
            
            Vector<short> iAVec = inputA.LoadVector(vectorIndex);
            Vector<short> dAVec = delta.LoadVector(offsetA + vectorIndex);
            Vector<short> rAVec = iAVec - dAVec;
            rAVec.ToArray(inputA, vectorIndex);
            
            Vector<short> iAVec2 = inputA.LoadVector(unrolledIndex);
            Vector<short> dAVec2 = delta.LoadVector(offsetA + unrolledIndex);
            Vector<short> rAVec2 = iAVec2 - dAVec2;
            rAVec2.ToArray(inputA, unrolledIndex);
            
            Vector<short> iAVec3 = inputA.LoadVector(unrolledIndex2);
            Vector<short> dAVec3 = delta.LoadVector(offsetA + unrolledIndex2);
            Vector<short> rAVec3 = iAVec3 - dAVec3;
            rAVec3.ToArray(inputA, unrolledIndex2);
            
            Vector<short> iAVec4 = inputA.LoadVector(unrolledIndex3);
            Vector<short> dAVec4 = delta.LoadVector(offsetA + unrolledIndex3);
            Vector<short> rAVec4 = iAVec4 - dAVec4;
            rAVec4.ToArray(inputA, unrolledIndex3);
            
            Vector<short> iBVec = inputB.LoadVector(vectorIndex);
            Vector<short> dBVec = delta.LoadVector(offsetB + vectorIndex);
            Vector<short> rBVec = iBVec - dBVec;
            rBVec.ToArray(inputB, vectorIndex);
            
            Vector<short> iBVec2 = inputB.LoadVector(unrolledIndex);
            Vector<short> dBVec2 = delta.LoadVector(offsetB + unrolledIndex);
            Vector<short> rBVec2 = iBVec2 - dBVec2;
            rBVec2.ToArray(inputB, unrolledIndex);
            
            Vector<short> iBVec3 = inputB.LoadVector(unrolledIndex2);
            Vector<short> dBVec3 = delta.LoadVector(offsetB + unrolledIndex2);
            Vector<short> rBVec3 = iBVec3 - dBVec3;
            rBVec3.ToArray(inputB, unrolledIndex2);
            
            Vector<short> iBVec4 = inputB.LoadVector(unrolledIndex3);
            Vector<short> dBVec4 = delta.LoadVector(offsetB + unrolledIndex3);
            Vector<short> rBVec4 = iBVec4 - dBVec4;
            rBVec4.ToArray(inputB, unrolledIndex3);

            vectorIndex = unrolledIndex3 + VSize.Short;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SubtractAndAddToAll(short[] input, short[] delta, int offsetS, int offsetA)
    {
        int size = input.Length;
        int loopSize = size / VSize.Short / UNROLL;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            int unrolledIndex = vectorIndex + VSize.Short;
            int unrolledIndex2 = unrolledIndex + VSize.Short;
            int unrolledIndex3 = unrolledIndex2 + VSize.Short;

            Vector<short> iVec = input.LoadVector(vectorIndex);
            Vector<short> dSVec = delta.LoadVector(offsetS + vectorIndex);
            Vector<short> dAVec = delta.LoadVector(offsetA + vectorIndex);
            Vector<short> rVec = iVec - dSVec + dAVec;
            rVec.ToArray(input, vectorIndex);
            
            Vector<short> iVec2 = input.LoadVector(unrolledIndex);
            Vector<short> dSVec2 = delta.LoadVector(offsetS + unrolledIndex);
            Vector<short> dAVec2 = delta.LoadVector(offsetA + unrolledIndex);
            Vector<short> rVec2 = iVec2 - dSVec2 + dAVec2;
            rVec2.ToArray(input, unrolledIndex);
            
            Vector<short> iVec3 = input.LoadVector(unrolledIndex2);
            Vector<short> dSVec3 = delta.LoadVector(offsetS + unrolledIndex2);
            Vector<short> dAVec3 = delta.LoadVector(offsetA + unrolledIndex2);
            Vector<short> rVec3 = iVec3 - dSVec3 + dAVec3;
            rVec3.ToArray(input, unrolledIndex2);
            
            Vector<short> iVec4 = input.LoadVector(unrolledIndex3);
            Vector<short> dSVec4 = delta.LoadVector(offsetS + unrolledIndex3);
            Vector<short> dAVec4 = delta.LoadVector(offsetA + unrolledIndex3);
            Vector<short> rVec4 = iVec4 - dSVec4 + dAVec4;
            rVec4.ToArray(input, unrolledIndex3);

            vectorIndex = unrolledIndex3 + VSize.Short;
        }
    }

}