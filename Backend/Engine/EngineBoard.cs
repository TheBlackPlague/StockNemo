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
    public Piece PieceOnly(Square sq) => Map.PieceOnly(sq);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRepetition() => History.Count(ZobristHash) > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GuiMove(Square from, Square to, Promotion promotion)
    {
        MoveList moveList = MoveList.WithoutProvidedPins(this, from);
        if (!moveList.Moves[to]) throw new InvalidOperationException("Invalid move provided by GUI.");
        if (promotion != Promotion.None && !moveList.Promotion) 
            throw new InvalidOperationException("Invalid move provided by GUI.");
        
        Move<NNUpdate>(from, to, promotion);
        History.Append(ZobristHash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RevertNullMove NullMove()
    {
        RevertNullMove rv = RevertNullMove.FromBitBoardMap(ref Map);
        
        if (Map.EnPassantTarget != Square.Na) Zobrist.HashEp(ref Map.ZobristHash, Map.EnPassantTarget);
        Map.EnPassantTarget = Square.Na;

        Map.ColorToMove = Map.ColorToMove.OppositeColor();
        Zobrist.FlipTurnInHash(ref Map.ZobristHash);

        return rv;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoNullMove(RevertNullMove rv)
    {
        if (rv.EnPassantTarget != Square.Na) {
            Map.EnPassantTarget = rv.EnPassantTarget;
            Zobrist.HashEp(ref Map.ZobristHash, rv.EnPassantTarget);
        }

        Map.ColorToMove = Map.ColorToMove.OppositeColor();
        Zobrist.FlipTurnInHash(ref Map.ZobristHash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RevertMove Move<MoveType>(ref OrderedMoveEntry move) where MoveType : MoveUpdateType
    {
        RevertMove rv;
        if (typeof(MoveType) == typeof(NNUpdate)) {
            Evaluation.NNUE.PushAccumulator();
            rv = Move<NNUpdate>(move.From, move.To, move.Promotion);
        } else {
            rv = Move<ClassicalUpdate>(move.From, move.To, move.Promotion);
        }
        History.Append(ZobristHash);
        return rv;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new void UndoMove<MoveType>(ref RevertMove rv) where MoveType : MoveUpdateType
    {
        if (typeof(MoveType) == typeof(NNUpdate)) {
            base.UndoMove<NNUpdate>(ref rv);
            Evaluation.NNUE.PullAccumulator();
        } else {
            base.UndoMove<ClassicalUpdate>(ref rv);
        }
        History.RemoveLast();
    }

    public new EngineBoard Clone() => new(this);

}