using System;
using System.Runtime.CompilerServices;
using Backend.Data;
using Backend.Data.Enum;
using Backend.Data.Struct;
using Backend.Data.Template;

namespace Backend.Engine;

public class EngineBoard : Board
{

    private readonly RepetitionHistory History;

    private EngineBoard(EngineBoard board) : base(board) => History = board.History.Clone();

    protected EngineBoard(string boardData, string turnData, string castlingData, string enPassantTargetData) :
        base(boardData, turnData, castlingData, enPassantTargetData)
    {
        History = new RepetitionHistory();
        Evaluation.NNUE.ResetAccumulator();
        Evaluation.NNUE.RefreshAccumulator(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRepetition() => History.Count(ZobristHash) > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GuiMove(Square from, Square to, Promotion promotion)
    {
        if (ColorToMove == PieceColor.White) {
            MoveList<White> moveList = MoveList<White>.WithoutProvidedPins(this, from);
            if (!moveList.Moves[to]) throw new InvalidOperationException("Invalid move provided by GUI.");
            if (promotion != Promotion.None && !moveList.Promotion) 
                throw new InvalidOperationException("Invalid move provided by GUI.");

            MoveNNUE<White>(from, to, promotion);
        } else {
            MoveList<Black> moveList = MoveList<Black>.WithoutProvidedPins(this, from);
            if (!moveList.Moves[to]) throw new InvalidOperationException("Invalid move provided by GUI.");
            if (promotion != Promotion.None && !moveList.Promotion) 
                throw new InvalidOperationException("Invalid move provided by GUI.");

            MoveNNUE<Black>(from, to, promotion);
        }
        
        History.Append(ZobristHash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RevertNullMove NullMove<ColorToMove>() where ColorToMove : Color
    {
        RevertNullMove rv = RevertNullMove.FromBitBoardMap(ref Map);
        
        if (Map.EnPassantTarget != Square.Na) Zobrist.HashEp(ref Map.ZobristHash, Map.EnPassantTarget);
        Map.EnPassantTarget = Square.Na;

        Map.ColorToMove = PieceColorUtil.OppositeColor<ColorToMove>();
        Zobrist.FlipTurnInHash(ref Map.ZobristHash);

        return rv;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoNullMove<ColorMoved>(RevertNullMove rv) where ColorMoved : Color
    {
        if (rv.EnPassantTarget != Square.Na) {
            Map.EnPassantTarget = rv.EnPassantTarget;
            Zobrist.HashEp(ref Map.ZobristHash, rv.EnPassantTarget);
        }

        Map.ColorToMove = PieceColorUtil.Color<ColorMoved>();
        Zobrist.FlipTurnInHash(ref Map.ZobristHash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RevertMove MoveNNUE<ColorToMove>(ref OrderedMoveEntry move) where ColorToMove : Color
    {
        Evaluation.NNUE.PushAccumulator();
        RevertMove rv = MoveNNUE<ColorToMove>(move.From, move.To, move.Promotion);
        History.Append(ZobristHash);
        return rv;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new void UndoMove<ColorMoved>(ref RevertMove rv) where ColorMoved : Color
    {
        base.UndoMove<ColorMoved>(ref rv);
        Evaluation.NNUE.PullAccumulator();
        History.RemoveLast();
    }

    public new EngineBoard Clone() => new(this);

}