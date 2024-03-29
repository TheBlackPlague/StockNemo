﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Data.Template;
using Backend.Engine.NNUE.Vectorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Backend.Engine.NNUE.Architecture.Basic;

[Serializable]
public class BasicNNUE
{

    private const int INPUT = 768;
    private const int HIDDEN = 256;
    private const int OUTPUT = 1;
    private const int CR_MIN = 0;
    private const int CR_MAX = 1 * QA;
    private const int SCALE = 400;

    private const int QA = 255;
    private const int QB = 64;
    private const int QAB = QA * QB;

    private const int ACCUMULATOR_STACK_SIZE = 512;

    private readonly short[] FeatureWeight = new short[INPUT * HIDDEN];
    private readonly short[] FeatureBias = new short[HIDDEN];
    private readonly short[] OutWeight = new short[HIDDEN * 2 * OUTPUT];
    private readonly short[] OutBias = new short[OUTPUT];

    private readonly short[] WhitePOV = new short[INPUT];
    private readonly short[] BlackPOV = new short[INPUT];

    private readonly BasicAccumulator<short>[] Accumulators = new BasicAccumulator<short>[ACCUMULATOR_STACK_SIZE];

    private readonly int[] Output = new int[OUTPUT];
    
    private int CurrentAccumulator;
    
    public BasicNNUE()
    {
        for (int i = 0; i < Accumulators.Length; i++) Accumulators[i] = new BasicAccumulator<short>(HIDDEN);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetAccumulator() => CurrentAccumulator = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushAccumulator()
    {
        Accumulators.AA(CurrentAccumulator).CopyTo(Accumulators.AA(++CurrentAccumulator));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PullAccumulator() => CurrentAccumulator--;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RefreshAccumulator(Board board)
    {
        Array.Clear(WhitePOV);
        Array.Clear(BlackPOV);

        BasicAccumulator<short> accumulator = Accumulators.AA(CurrentAccumulator);
        accumulator.Zero();
        accumulator.PreLoadBias(FeatureBias);

        for (Square sq = Square.A1; sq < Square.Na; sq++) {
            (Piece piece, PieceColor color) = board.At(sq);
            if (piece == Piece.Empty || color == PieceColor.None) continue;
            
            EfficientlyUpdateAccumulator<Activate>(piece, color, sq);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void EfficientlyUpdateAccumulator(Piece piece, PieceColor color, Square from, Square to)
    {
        const int colorStride = 64 * 6;
        const int pieceStride = 64;

        Piece nnPiece = NN.PieceToNN(piece);
        int opPieceStride = (int)nnPiece * pieceStride;

        int whiteIndexFrom = (int)color * colorStride + opPieceStride + (int)from;
        int blackIndexFrom = (int)color.OppositeColor() * colorStride + opPieceStride + ((int)from ^ 56);
        int whiteIndexTo = (int)color * colorStride + opPieceStride + (int)to;
        int blackIndexTo = (int)color.OppositeColor() * colorStride + opPieceStride + ((int)to ^ 56);

        BasicAccumulator<short> accumulator = Accumulators.AA(CurrentAccumulator);

        WhitePOV.AA(whiteIndexFrom) = 0;
        BlackPOV.AA(blackIndexFrom) = 0;
        WhitePOV.AA(whiteIndexTo) = 1;
        BlackPOV.AA(blackIndexTo) = 1;
        
        NN.SubtractAndAddToAll(accumulator.White, FeatureWeight, 
            whiteIndexFrom * HIDDEN, whiteIndexTo * HIDDEN);
        NN.SubtractAndAddToAll(accumulator.Black, FeatureWeight, 
            blackIndexFrom * HIDDEN, blackIndexTo * HIDDEN);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void EfficientlyUpdateAccumulator<Operation>(Piece piece, PieceColor color, Square sq)
        where Operation : AccumulatorOperation
    {
        const int colorStride = 64 * 6;
        const int pieceStride = 64;

        Piece nnPiece = NN.PieceToNN(piece);
        int opPieceStride = (int)nnPiece * pieceStride;

        int whiteIndex = (int)color * colorStride + opPieceStride + (int)sq;
        int blackIndex = (int)color.OppositeColor() * colorStride + opPieceStride + ((int)sq ^ 56);

        BasicAccumulator<short> accumulator = Accumulators.AA(CurrentAccumulator);

        if (typeof(Operation) == typeof(Activate)) {
            WhitePOV.AA(whiteIndex) = 1;
            BlackPOV.AA(blackIndex) = 1;
            NN.AddToAll(accumulator.White, accumulator.Black, FeatureWeight, 
                whiteIndex * HIDDEN, blackIndex * HIDDEN);
        } else {
            WhitePOV.AA(whiteIndex) = 0;
            BlackPOV.AA(blackIndex) = 0;
            NN.SubtractFromAll(accumulator.White, accumulator.Black, FeatureWeight, 
                whiteIndex * HIDDEN, blackIndex * HIDDEN);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int Evaluate(PieceColor colorToMove)
    {
        BasicAccumulator<short> accumulator = Accumulators.AA(CurrentAccumulator);

        if (colorToMove == PieceColor.White) {
            NN.ClippedReLUFlattenAndForward(accumulator.White, accumulator.Black, OutWeight, Output, 
                CR_MIN, CR_MAX, HIDDEN);
        } else {
            NN.ClippedReLUFlattenAndForward(accumulator.Black, accumulator.White, OutWeight, Output, 
                CR_MIN, CR_MAX, HIDDEN);
        }
        
        return (Output.AA(0) + OutBias.AA(0)) * SCALE / QAB;
    }

    #region JSON

    public void FromJson(Stream stream)
    {
        using JsonTextReader reader = new(new StreamReader(stream));
        
        JObject jsonObject = JObject.Load(reader);
        foreach (KeyValuePair<string, JToken> property in jsonObject) {
            switch (property.Key) {
                case "ft.weight":
                    Weight(property.Value, FeatureWeight, HIDDEN, QA, true);
                    Console.WriteLine("Feature weights loaded.");
                    break;
                case "ft.bias":
                    Bias(property.Value, FeatureBias, QA);
                    Console.WriteLine("Feature bias loaded.");
                    break;
                case "out.weight":
                    Weight(property.Value, OutWeight, HIDDEN * 2, QB);
                    Console.WriteLine("Out weights loaded.");
                    break;
                case "out.bias":
                    Bias(property.Value, OutBias, QAB);
                    Console.WriteLine("Out bias loaded.");
                    break;
            }
        }
        
        Console.WriteLine("BasicNNUE loaded from JSON.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Weight(JToken weightRelation, short[] weightArray, int stride, int k, bool flip = false)
        {
            int i = 0;
            foreach (JToken output in weightRelation) {
                int j = 0;
                foreach (JToken weight in output) {
                    int index;
                    if (flip) index = j * stride + i;
                    else index = i * stride + j;
                    double value = weight.ToObject<double>();
                    weightArray.AA(index) = (short)(value * k);
                    j++;
                }
                i++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Bias(JToken biasRelation, short[] biasArray, int k)
        {
            int i = 0;
            foreach (JToken bias in biasRelation) {
                double value = bias.ToObject<double>();
                biasArray.AA(i) = (short)(value * k);
                i++;
            }
        }
    }

    #endregion

}