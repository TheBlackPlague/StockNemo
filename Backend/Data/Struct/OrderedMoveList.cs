using System;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data.Struct;

public readonly ref struct OrderedMoveList
{
    
    private const int PRIORITY = int.MaxValue;

    private static readonly int[][] MvvLvaTable =
    {
        new[] { 2005, 2002, 2004, 2003, 2001, 2000 },
        new[] { 3005, 3002, 3004, 3003, 3001, 3000 },
        new[] { 4005, 4002, 4004, 4003, 4001, 4000 },
        new[] { 5005, 5002, 5004, 5003, 5001, 5000 },
        new[] { 6005, 6002, 6004, 6003, 6001, 6000 },
        new[] { 7005, 7002, 7004, 7003, 7001, 7000 }
    };

    public int Count { get; }
    
    private readonly Span<OrderedMoveEntry> Internal;
    
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static int ScoreMove(
        Board board, 
        ref OrderedMoveEntry move,
        SearchedMove tableMove
        )
    {
        if (move == tableMove) {
            return PRIORITY - 1;
        }
        
        if (move.Promotion != Promotion.None) {
            return PRIORITY - 8 + (int)move.Promotion;
        }

        Piece to = board.At(move.To).Item1;
        if (to != Piece.Empty) {
            return MvvLva(board.At(move.From).Item1, to) * 1000;
        }

        return 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int MvvLva(Piece attacker, Piece victim) => MvvLvaTable[(int)victim][(int)attacker];

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public OrderedMoveList(
        Board board, 
        ref Span<OrderedMoveEntry> memory, 
        SearchedMove transpositionMove
        )
    {
        Internal = memory;

        PieceColor color = board.WhiteTurn ? PieceColor.White : PieceColor.Black;
        PieceColor oppositeColor = Util.OppositeColor(color);

        // Generate pins and check bitboards.
        Square kingSq = board.KingLoc(color);
        (BitBoard hv, BitBoard d) = MoveList.PinBitBoards(board, kingSq, color, oppositeColor);
        (BitBoard checks, bool doubleChecked) = MoveList.CheckBitBoard(board, kingSq, oppositeColor);

        // Define the list.
        int i = 0;
        BitBoardIterator fromIterator;
        Square from;
        if (!doubleChecked) {
            // We can only do this if we're not double checked.
            // In case of double-checked (discovered + normal), only the king can move so we should skip this.
            
            // Generate all pawn moves.
            fromIterator = board.All(Piece.Pawn, color).GetEnumerator();
            from = fromIterator.Current;
            while (fromIterator.MoveNext()) {
                MoveList moveList = new(
                    board, from, Piece.Pawn, color, 
                    ref hv, ref d, ref checks, false
                );
                BitBoardIterator moves = moveList.Moves.GetEnumerator();
                Square move = moves.Current;
                
                while (moves.MoveNext()) {
                    if (moveList.Promotion) {
                        int p = 1;
                        while (p < 5) {
                            Internal[i] = new OrderedMoveEntry(from, move, (Promotion)p);
                            Internal[i].Score = ScoreMove(board, ref Internal[i], transpositionMove);
                            i++;
                            p++;
                        }
                    } else {
                        Internal[i] = new OrderedMoveEntry(from, move, Promotion.None);
                        Internal[i].Score = ScoreMove(board, ref Internal[i], transpositionMove);
                        i++;
                    }
                    
                    move = moves.Current;
                }
                
                from = fromIterator.Current;
            }

            // Generate moves for rook, knight, bishop, and queen.
            sbyte piece = 1;
            while (piece < 5) {
                fromIterator = board.All((Piece)piece, color).GetEnumerator();
                from = fromIterator.Current;
                while (fromIterator.MoveNext()) {
                    MoveList moveList = new(
                        board, from, (Piece)piece, color, 
                        ref hv, ref d, ref checks, false
                    );
                    BitBoardIterator moves = moveList.Moves.GetEnumerator();
                    Square move = moves.Current;

                    while (moves.MoveNext()) {
                        Internal[i] = new OrderedMoveEntry(from, move, Promotion.None);
                        Internal[i].Score = ScoreMove(board, ref Internal[i], transpositionMove);
                        i++;
                    
                        move = moves.Current;
                    }

                    from = fromIterator.Current;
                }

                piece++;
            }
        }
        
        // Generate all king moves.
        fromIterator = board.All(Piece.King, color).GetEnumerator();
        from = fromIterator.Current;
        while (fromIterator.MoveNext()) {
            MoveList moveList = new(
                board, from, Piece.King, color, 
                ref hv, ref d, ref checks, false
            );
            BitBoardIterator moves = moveList.Moves.GetEnumerator();
            Square move = moves.Current;

            while (moves.MoveNext()) {
                Internal[i] = new OrderedMoveEntry(from, move, Promotion.None);
                Internal[i].Score = ScoreMove(board, ref Internal[i], transpositionMove);
                i++;
                    
                move = moves.Current;
            }

            from = fromIterator.Current;
        }

        Count = i;
    }

    public ref OrderedMoveEntry this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Internal[i];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SortNext(int sorted)
    {
        int index = sorted;
        int i = 1 + sorted;
        while (i < Count) {
            if (Internal[i].Score > Internal[index].Score) index = i;
            i++;
        }
        
        Swap(index, sorted);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Swap(int firstIndex, int secondIndex) => (Internal[firstIndex], Internal[secondIndex]) =
        (Internal[secondIndex], Internal[firstIndex]);

}