using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Data.Struct;
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
    private const int CR_MAX = 1;
    private const int SCALE = 400;

    private readonly double[] FeatureWeight = new double[INPUT * HIDDEN];
    private readonly double[] FeatureBias = new double[HIDDEN];
    private readonly double[] OutWeight = new double[HIDDEN * 2 * OUTPUT];
    private readonly double[] OutBias = new double[OUTPUT];

    private readonly double[] WhitePOV = new double[INPUT];
    private readonly double[] BlackPOV = new double[INPUT];

    private readonly BasicAccumulator Accumulator = new(HIDDEN);

    private readonly double[] Flatten = new double[HIDDEN * 2];

    private readonly double[] Output = new double[OUTPUT];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RefreshAccumulator(Board board)
    {
        const int colorStride = 64 * 6;
        const int pieceStride = 64;
        
        Array.Clear(WhitePOV);
        Array.Clear(BlackPOV);
        
        for (PieceColor color = PieceColor.White; color < PieceColor.None; color++)
        for (Piece piece = Piece.Pawn; piece < Piece.Empty; piece++) {
            BitBoardIterator whiteIterator = board.All(piece, color).GetEnumerator();
            BitBoardIterator blackIterator = board.All(piece, color).GetEnumerator();
            Piece originalPiece = piece;
            if (piece == Piece.Rook) piece += 2;
            else if (piece == Piece.Knight || piece == Piece.Bishop) piece--;

            Square sq = whiteIterator.Current;
            while (whiteIterator.MoveNext()) {
                int index = (int)color * colorStride + (int)piece * pieceStride + (int)sq;
                WhitePOV.AA(index) = 1;
                sq = whiteIterator.Current;
            }

            sq = blackIterator.Current;
            while (blackIterator.MoveNext()) {
                int index = (int)Util.OppositeColor(color) * colorStride + (int)piece * pieceStride + ((int)sq ^ 56);
                BlackPOV.AA(index) = 1;
                sq = blackIterator.Current;
            }

            piece = originalPiece;
        }
        
        NN.Forward(WhitePOV, FeatureWeight, Accumulator.A);
        NN.Forward(BlackPOV, FeatureWeight, Accumulator.B);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Evaluate(PieceColor colorToMove)
    {
        int firstOffset = 0;
        int secondOffset = 256;

        if (colorToMove == PieceColor.Black) {
            firstOffset = 256;
            secondOffset = 0;
        }
        
        NN.ClippedReLU(Accumulator.A, FeatureBias, Flatten, CR_MIN, CR_MAX, firstOffset);
        NN.ClippedReLU(Accumulator.B, FeatureBias, Flatten, CR_MIN, CR_MAX, secondOffset);
        
        NN.Forward(Flatten, OutWeight, Output);
        return (Output.AA(0) + OutBias.AA(0)) * SCALE;
    }

    #region JSON

    public void FromJson(Stream stream)
    {
        using JsonTextReader reader = new(new StreamReader(stream));
        
        JObject jsonObject = JObject.Load(reader);
        foreach (KeyValuePair<string, JToken> property in jsonObject) {
            switch (property.Key) {
                case "ft.weight":
                    Weight(property.Value, FeatureWeight, INPUT);
                    Console.WriteLine("Feature weights loaded.");
                    break;
                case "ft.bias":
                    Bias(property.Value, FeatureBias);
                    Console.WriteLine("Feature bias loaded.");
                    break;
                case "out.weight":
                    Weight(property.Value, OutWeight, HIDDEN * 2);
                    Console.WriteLine("Out weights loaded.");
                    break;
                case "out.bias":
                    Bias(property.Value, OutBias);
                    Console.WriteLine("Out bias loaded.");
                    break;
            }
        }
        
        Console.WriteLine("BasicNNUE loaded from JSON.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Weight(JToken weightRelation, double[] weightArray, int stride)
        {
            int i = 0;
            foreach (JToken output in weightRelation) {
                int j = 0;
                foreach (JToken weight in output) {
                    int index = i * stride + j;
                    double value = weight.ToObject<double>();
                    weightArray.AA(index) = value;
                    j++;
                }
                i++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Bias(JToken biasRelation, double[] biasArray)
        {
            int i = 0;
            foreach (JToken bias in biasRelation) {
                double value = bias.ToObject<double>();
                biasArray.AA(i) = value;
                i++;
            }
        }
    }

    #endregion

}