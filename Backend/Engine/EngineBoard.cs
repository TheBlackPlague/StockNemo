using System.Runtime.CompilerServices;
using Backend.Data;
using Backend.Data.Struct;

namespace Backend.Engine;

public class EngineBoard : Board
{

    private readonly HashHistory History;
    
    public new static EngineBoard Default()
    {
        return FromFen(DEFAULT_FEN);
    }

    public new static EngineBoard FromFen(string fen)
    {
        string[] parts = fen.Split(" ");
        return new EngineBoard(parts[0], parts[1], parts[2], parts[3]);
    }

    private EngineBoard(EngineBoard board) : base(board) => History = board.History.Clone();

    protected EngineBoard(string boardData, string turnData, string castlingData, string enPassantTargetData) :
        base(boardData, turnData, castlingData, enPassantTargetData) => History = new HashHistory();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRepetition() => History.Count(ZobristHash) > 2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RevertMove Move(ref OrderedMoveEntry move)
    {
        History.Append(ZobristHash);
        return Move(move.From, move.To, move.Promotion);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new void UndoMove(ref RevertMove rv)
    {
        base.UndoMove(ref rv);
        History.RemoveLast();
    }

    public new EngineBoard Clone() => new(this);

}