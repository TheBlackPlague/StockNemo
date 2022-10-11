using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void ClippedReLUFlattenAndForward(short[] inputA, short[] inputB, short[] bias, short[] weight, 
        int[] output, short min, short max, int separationIndex, int offset = 0)
    {
        int inputSize = inputA.Length + inputB.Length;
        int loopSize = inputSize / VSize.Short / UNROLL;
        int outputSize = output.Length;
        int weightStride = 0;
        
        Vector<short> minVec = new(min);
        Vector<short> maxVec = new(max);

        ref short inputAReference = ref MemoryMarshal.GetArrayDataReference(inputA);
        ref short inputBReference = ref MemoryMarshal.GetArrayDataReference(inputB);
        ref short biasReference = ref MemoryMarshal.GetArrayDataReference(bias);
        ref short weightReference = ref MemoryMarshal.GetArrayDataReference(weight);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int RelativeIndex(int index) => index < separationIndex ? index : index - separationIndex;

        for (int i = 0; i < outputSize; i++) {
            Vector<int> sum = Vector<int>.Zero;
            int vectorIndex = 0;
            for (int j = 0; j < loopSize; j++) {
                ref short inputReference = ref vectorIndex < separationIndex ? 
                    ref inputAReference : ref inputBReference;

                int unrolledIndex = vectorIndex + VSize.Short;
                int unrolledIndex2 = unrolledIndex + VSize.Short;
                int unrolledIndex3 = unrolledIndex2 + VSize.Short;
                
                int rIndex = RelativeIndex(vectorIndex);
                int unrolledRIndex = rIndex + VSize.Short;
                int unrolledRIndex2 = unrolledRIndex + VSize.Short;
                int unrolledRIndex3 = unrolledRIndex2 + VSize.Short;

                Vector<short> iVec = inputReference.LoadVector(rIndex);
                Vector<short> bVec = biasReference.LoadVector(rIndex);
                Vector<short> wVec = weightReference.LoadVector(weightStride + vectorIndex);
                Vector<short> clamped = (iVec + bVec).Clamp(ref minVec, ref maxVec);
                sum += VMethod.MultiplyAddAdjacent(clamped, wVec);
                
                Vector<short> iVec2 = inputReference.LoadVector(unrolledRIndex);
                Vector<short> bVec2 = biasReference.LoadVector(unrolledRIndex);
                Vector<short> wVec2 = weightReference.LoadVector(weightStride + unrolledIndex);
                Vector<short> clamped2 = (iVec2 + bVec2).Clamp(ref minVec, ref maxVec);
                sum += VMethod.MultiplyAddAdjacent(clamped2, wVec2);
                
                Vector<short> iVec3 = inputReference.LoadVector(unrolledRIndex2);
                Vector<short> bVec3 = biasReference.LoadVector(unrolledRIndex2);
                Vector<short> wVec3 = weightReference.LoadVector(weightStride + unrolledIndex2);
                Vector<short> clamped3 = (iVec3 + bVec3).Clamp(ref minVec, ref maxVec);
                sum += VMethod.MultiplyAddAdjacent(clamped3, wVec3);
                
                Vector<short> iVec4 = inputReference.LoadVector(unrolledRIndex3);
                Vector<short> bVec4 = biasReference.LoadVector(unrolledRIndex3);
                Vector<short> wVec4 = weightReference.LoadVector(weightStride + unrolledIndex3);
                Vector<short> clamped4 = (iVec4 + bVec4).Clamp(ref minVec, ref maxVec);
                sum += VMethod.MultiplyAddAdjacent(clamped4, wVec4);
                
                vectorIndex = unrolledIndex3 + VSize.Short;
            }
            
            output.AA(offset + i) = Vector.Sum(sum);
            weightStride += inputSize;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToAll(short[] inputA, short[] inputB, short[] delta, int offsetA, int offsetB)
    {
        int size = inputA.Length;
        int loopSize = size / VSize.Short / UNROLL;
        
        ref short inputAReference = ref MemoryMarshal.GetArrayDataReference(inputA);
        ref short inputBReference = ref MemoryMarshal.GetArrayDataReference(inputB);
        ref short deltaReference = ref MemoryMarshal.GetArrayDataReference(delta);

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            int unrolledIndex = vectorIndex + VSize.Short;
            int unrolledIndex2 = unrolledIndex + VSize.Short;
            int unrolledIndex3 = unrolledIndex2 + VSize.Short;
            
            Vector<short> iAVec = inputAReference.LoadVector(vectorIndex);
            Vector<short> dAVec = deltaReference.LoadVector(offsetA + vectorIndex);
            Vector<short> rAVec = iAVec + dAVec;
            rAVec.ToArray(ref inputAReference, vectorIndex);
            
            Vector<short> iAVec2 = inputAReference.LoadVector(unrolledIndex);
            Vector<short> dAVec2 = deltaReference.LoadVector(offsetA + unrolledIndex);
            Vector<short> rAVec2 = iAVec2 + dAVec2;
            rAVec2.ToArray(ref inputAReference, unrolledIndex);
            
            Vector<short> iAVec3 = inputAReference.LoadVector(unrolledIndex2);
            Vector<short> dAVec3 = deltaReference.LoadVector(offsetA + unrolledIndex2);
            Vector<short> rAVec3 = iAVec3 + dAVec3;
            rAVec3.ToArray(ref inputAReference, unrolledIndex2);
            
            Vector<short> iAVec4 = inputAReference.LoadVector(unrolledIndex3);
            Vector<short> dAVec4 = deltaReference.LoadVector(offsetA + unrolledIndex3);
            Vector<short> rAVec4 = iAVec4 + dAVec4;
            rAVec4.ToArray(ref inputAReference, unrolledIndex3);
            
            Vector<short> iBVec = inputBReference.LoadVector(vectorIndex);
            Vector<short> dBVec = deltaReference.LoadVector(offsetB + vectorIndex);
            Vector<short> rBVec = iBVec + dBVec;
            rBVec.ToArray(ref inputBReference, vectorIndex);
            
            Vector<short> iBVec2 = inputBReference.LoadVector(unrolledIndex);
            Vector<short> dBVec2 = deltaReference.LoadVector(offsetB + unrolledIndex);
            Vector<short> rBVec2 = iBVec2 + dBVec2;
            rBVec2.ToArray(ref inputBReference, unrolledIndex);
            
            Vector<short> iBVec3 = inputBReference.LoadVector(unrolledIndex2);
            Vector<short> dBVec3 = deltaReference.LoadVector(offsetB + unrolledIndex2);
            Vector<short> rBVec3 = iBVec3 + dBVec3;
            rBVec3.ToArray(ref inputBReference, unrolledIndex2);
            
            Vector<short> iBVec4 = inputBReference.LoadVector(unrolledIndex3);
            Vector<short> dBVec4 = deltaReference.LoadVector(offsetB + unrolledIndex3);
            Vector<short> rBVec4 = iBVec4 + dBVec4;
            rBVec4.ToArray(ref inputBReference, unrolledIndex3);

            vectorIndex = unrolledIndex3 + VSize.Short;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SubtractFromAll(short[] inputA, short[] inputB, short[] delta, int offsetA, int offsetB)
    {
        int size = inputA.Length;
        int loopSize = size / VSize.Short / UNROLL;
        
        ref short inputAReference = ref MemoryMarshal.GetArrayDataReference(inputA);
        ref short inputBReference = ref MemoryMarshal.GetArrayDataReference(inputB);
        ref short deltaReference = ref MemoryMarshal.GetArrayDataReference(delta);

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            int unrolledIndex = vectorIndex + VSize.Short;
            int unrolledIndex2 = unrolledIndex + VSize.Short;
            int unrolledIndex3 = unrolledIndex2 + VSize.Short;
            
            Vector<short> iAVec = inputAReference.LoadVector(vectorIndex);
            Vector<short> dAVec = deltaReference.LoadVector(offsetA + vectorIndex);
            Vector<short> rAVec = iAVec - dAVec;
            rAVec.ToArray(ref inputAReference, vectorIndex);
            
            Vector<short> iAVec2 = inputAReference.LoadVector(unrolledIndex);
            Vector<short> dAVec2 = deltaReference.LoadVector(offsetA + unrolledIndex);
            Vector<short> rAVec2 = iAVec2 - dAVec2;
            rAVec2.ToArray(ref inputAReference, unrolledIndex);
            
            Vector<short> iAVec3 = inputAReference.LoadVector(unrolledIndex2);
            Vector<short> dAVec3 = deltaReference.LoadVector(offsetA + unrolledIndex2);
            Vector<short> rAVec3 = iAVec3 - dAVec3;
            rAVec3.ToArray(ref inputAReference, unrolledIndex2);
            
            Vector<short> iAVec4 = inputAReference.LoadVector(unrolledIndex3);
            Vector<short> dAVec4 = deltaReference.LoadVector(offsetA + unrolledIndex3);
            Vector<short> rAVec4 = iAVec4 - dAVec4;
            rAVec4.ToArray(ref inputAReference, unrolledIndex3);
            
            Vector<short> iBVec = inputBReference.LoadVector(vectorIndex);
            Vector<short> dBVec = deltaReference.LoadVector(offsetB + vectorIndex);
            Vector<short> rBVec = iBVec - dBVec;
            rBVec.ToArray(ref inputBReference, vectorIndex);
            
            Vector<short> iBVec2 = inputBReference.LoadVector(unrolledIndex);
            Vector<short> dBVec2 = deltaReference.LoadVector(offsetB + unrolledIndex);
            Vector<short> rBVec2 = iBVec2 - dBVec2;
            rBVec2.ToArray(ref inputBReference, unrolledIndex);
            
            Vector<short> iBVec3 = inputBReference.LoadVector(unrolledIndex2);
            Vector<short> dBVec3 = deltaReference.LoadVector(offsetB + unrolledIndex2);
            Vector<short> rBVec3 = iBVec3 - dBVec3;
            rBVec3.ToArray(ref inputBReference, unrolledIndex2);
            
            Vector<short> iBVec4 = inputBReference.LoadVector(unrolledIndex3);
            Vector<short> dBVec4 = deltaReference.LoadVector(offsetB + unrolledIndex3);
            Vector<short> rBVec4 = iBVec4 - dBVec4;
            rBVec4.ToArray(ref inputBReference, unrolledIndex3);

            vectorIndex = unrolledIndex3 + VSize.Short;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SubtractAndAddToAll(short[] input, short[] delta, int offsetS, int offsetA)
    {
        int size = input.Length;
        int loopSize = size / VSize.Short / UNROLL;

        ref short inputReference = ref MemoryMarshal.GetArrayDataReference(input);
        ref short deltaReference = ref MemoryMarshal.GetArrayDataReference(delta);

        int vectorIndex = 0;
        for (int i = 0; i < loopSize; i++) {
            int unrolledIndex = vectorIndex + VSize.Short;
            int unrolledIndex2 = unrolledIndex + VSize.Short;
            int unrolledIndex3 = unrolledIndex2 + VSize.Short;

            Vector<short> iVec = inputReference.LoadVector(vectorIndex);
            Vector<short> dSVec = deltaReference.LoadVector(offsetS + vectorIndex);
            Vector<short> dAVec = deltaReference.LoadVector(offsetA + vectorIndex);
            Vector<short> rVec = iVec - dSVec + dAVec;
            rVec.ToArray(ref inputReference, vectorIndex);
            
            Vector<short> iVec2 = inputReference.LoadVector(unrolledIndex);
            Vector<short> dSVec2 = deltaReference.LoadVector(offsetS + unrolledIndex);
            Vector<short> dAVec2 = deltaReference.LoadVector(offsetA + unrolledIndex);
            Vector<short> rVec2 = iVec2 - dSVec2 + dAVec2;
            rVec2.ToArray(ref inputReference, unrolledIndex);
            
            Vector<short> iVec3 = inputReference.LoadVector(unrolledIndex2);
            Vector<short> dSVec3 = deltaReference.LoadVector(offsetS + unrolledIndex2);
            Vector<short> dAVec3 = deltaReference.LoadVector(offsetA + unrolledIndex2);
            Vector<short> rVec3 = iVec3 - dSVec3 + dAVec3;
            rVec3.ToArray(ref inputReference, unrolledIndex2);
            
            Vector<short> iVec4 = inputReference.LoadVector(unrolledIndex3);
            Vector<short> dSVec4 = deltaReference.LoadVector(offsetS + unrolledIndex3);
            Vector<short> dAVec4 = deltaReference.LoadVector(offsetA + unrolledIndex3);
            Vector<short> rVec4 = iVec4 - dSVec4 + dAVec4;
            rVec4.ToArray(ref inputReference, unrolledIndex3);

            vectorIndex = unrolledIndex3 + VSize.Short;
        }
    }

}