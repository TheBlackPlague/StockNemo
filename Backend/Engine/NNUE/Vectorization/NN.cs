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
            short sum = 0;
            int vectorIndex = 0;
            for (int j = 0; j < loopSize; j++) {
                int unrolledIndex = vectorIndex + VSize.Short;
                int unrolledIndex2 = unrolledIndex + VSize.Short;
                int unrolledIndex3 = unrolledIndex2 + VSize.Short;
                
                Vector<short> iVec = input.NewVector(vectorIndex);
                Vector<short> wVec = weight.NewVector(weightStride + vectorIndex);
                sum += Vector.Sum(iVec * wVec);
                
                Vector<short> iVec2 = input.NewVector(unrolledIndex);
                Vector<short> wVec2 = weight.NewVector(weightStride + unrolledIndex);
                sum += Vector.Sum(iVec2 * wVec2);
                
                Vector<short> iVec3 = input.NewVector(unrolledIndex2);
                Vector<short> wVec3 = weight.NewVector(weightStride + unrolledIndex2);
                sum += Vector.Sum(iVec3 * wVec3);
                
                Vector<short> iVec4 = input.NewVector(unrolledIndex3);
                Vector<short> wVec4 = weight.NewVector(weightStride + unrolledIndex3);
                sum += Vector.Sum(iVec4 * wVec4);
                
                vectorIndex = unrolledIndex3 + VSize.Short;
            }
            output.AA(offset + i) = sum;
            weightStride += inputSize;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forward(short[] input, short[] weight, int[] output, int offset = 0)
    {
        int inputSize = input.Length;
        int loopSize = inputSize / VSize.Short / UNROLL;
        int outputSize = output.Length;
        int weightStride = 0;

        for (int i = 0; i < outputSize; i++) {
            int sum = 0;
            int vectorIndex = 0;
            for (int j = 0; j < loopSize; j++) {
                int unrolledIndex = vectorIndex + VSize.Short;
                int unrolledIndex2 = unrolledIndex + VSize.Short;
                int unrolledIndex3 = unrolledIndex2 + VSize.Short;
                
                Vector<short> iVec = input.NewVector(vectorIndex);
                Vector<short> wVec = weight.NewVector(weightStride + vectorIndex);
                sum += Vector.Sum(VMethod.MultiplyAddAdjacent(iVec, wVec));
                
                Vector<short> iVec2 = input.NewVector(unrolledIndex);
                Vector<short> wVec2 = weight.NewVector(weightStride + unrolledIndex);
                sum += Vector.Sum(VMethod.MultiplyAddAdjacent(iVec2, wVec2));
                
                Vector<short> iVec3 = input.NewVector(unrolledIndex2);
                Vector<short> wVec3 = weight.NewVector(weightStride + unrolledIndex2);
                sum += Vector.Sum(VMethod.MultiplyAddAdjacent(iVec3, wVec3));
                
                Vector<short> iVec4 = input.NewVector(unrolledIndex3);
                Vector<short> wVec4 = weight.NewVector(weightStride + unrolledIndex3);
                sum += Vector.Sum(VMethod.MultiplyAddAdjacent(iVec4, wVec4));
                
                vectorIndex = unrolledIndex3 + VSize.Short;
            }
            output.AA(offset + i) = sum;
            weightStride += inputSize;
        }
    }
    
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToAll(short[] input, short[] delta, int offset)
    {
        int size = input.Length;
        int loopSize = size / VSize.Short / UNROLL;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            int unrolledIndex = vectorIndex + VSize.Short;
            int unrolledIndex2 = unrolledIndex + VSize.Short;
            int unrolledIndex3 = unrolledIndex2 + VSize.Short;
            
            Vector<short> iVec = input.NewVector(vectorIndex);
            Vector<short> dVec = delta.NewVector(offset + vectorIndex);
            Vector<short> rVec = iVec + dVec;
            rVec.ToArray(input, vectorIndex);
            
            Vector<short> iVec2 = input.NewVector(unrolledIndex);
            Vector<short> dVec2 = delta.NewVector(offset + unrolledIndex);
            Vector<short> rVec2 = iVec2 + dVec2;
            rVec2.ToArray(input, unrolledIndex);
            
            Vector<short> iVec3 = input.NewVector(unrolledIndex2);
            Vector<short> dVec3 = delta.NewVector(offset + unrolledIndex2);
            Vector<short> rVec3 = iVec3 + dVec3;
            rVec3.ToArray(input, unrolledIndex2);
            
            Vector<short> iVec4 = input.NewVector(unrolledIndex3);
            Vector<short> dVec4 = delta.NewVector(offset + unrolledIndex3);
            Vector<short> rVec4 = iVec4 + dVec4;
            rVec4.ToArray(input, unrolledIndex3);

            vectorIndex = unrolledIndex3 + VSize.Short;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SubtractFromAll(short[] input, short[] delta, int offset)
    {
        int size = input.Length;
        int loopSize = size / VSize.Short / UNROLL;

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            int unrolledIndex = vectorIndex + VSize.Short;
            int unrolledIndex2 = unrolledIndex + VSize.Short;
            int unrolledIndex3 = unrolledIndex2 + VSize.Short;
            
            Vector<short> iVec = input.NewVector(vectorIndex);
            Vector<short> dVec = delta.NewVector(offset + vectorIndex);
            Vector<short> rVec = iVec - dVec;
            rVec.ToArray(input, vectorIndex);
            
            Vector<short> iVec2 = input.NewVector(unrolledIndex);
            Vector<short> dVec2 = delta.NewVector(offset + unrolledIndex);
            Vector<short> rVec2 = iVec2 - dVec2;
            rVec2.ToArray(input, unrolledIndex);
            
            Vector<short> iVec3 = input.NewVector(unrolledIndex2);
            Vector<short> dVec3 = delta.NewVector(offset + unrolledIndex2);
            Vector<short> rVec3 = iVec3 - dVec3;
            rVec3.ToArray(input, unrolledIndex2);
            
            Vector<short> iVec4 = input.NewVector(unrolledIndex3);
            Vector<short> dVec4 = delta.NewVector(offset + unrolledIndex3);
            Vector<short> rVec4 = iVec4 - dVec4;
            rVec4.ToArray(input, unrolledIndex3);

            vectorIndex = unrolledIndex3 + VSize.Short;
        }
    }

}