using System;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data.Struct;

public readonly ref struct OrderedMoveList
{

    // Technically, there do exist positions where we'd have 218 legal moves.
    // However, they are so unlikely that 128 seems like an okay number.
    public const int SIZE = 128;
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
    
    private readonly Span<OrderedMoveEntry> Internal;

    private readonly OrderedMoveEntry KillerMoveOne;
    private readonly OrderedMoveEntry KillerMoveTwo;

    private readonly HistoryTable HistoryTable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ScoreMoveNormal(
        Piece pieceToMove,
        Board board, 
        ref OrderedMoveEntry move,
        SearchedMove tableMove
        )
    {
        // Compare our move with the one found from transposition table. There's no guarantee the transposition move
        // is even legal, so this acts as a sort of legal verification for it too.
        // Regardless, if our move is equal to that (also proving that it is legal for this position), then give it
        // highest priority, making it the first move we make.
        if (move == tableMove) return PRIORITY - 1;
        
        // Score promotions based on the type of promotion it is. 
        // Promotion | Score
        // Queen     | PRIORITY - 4 (HIGHEST)
        // Bishop    | PRIORITY - 5
        // Knight    | PRIORITY - 6
        // Rook      | PRIORITY - 7 (LOWEST)
        if (move.Promotion != Promotion.None) return PRIORITY - 8 + (int)move.Promotion;

        // Score captures based on the piece capturing and the piece being captured.
        // The idea behind it is to give highest priority to captures that are capturing most valuable pieces
        // with least valuable pieces.
        Piece to = board.At(move.To).Item1;
        if (to != Piece.Empty) return MvvLva(board.At(move.From).Item1, to) * 10000;

        // If the move is a quiet move (not capture / promotion), then we should check if it is a killer move or history
        // move.
        // Killer moves are moves that are very likely to cause a beta cutoff.
        // History moves are moves that have been scored as most likely alpha changing moves, depending on how many
        // times the move changed alpha (gave us a guaranteed best move).
        
        // Check if move is a rank 1 killer move (extremely local, recently updated).
        if (move == KillerMoveOne) return 900000;
        // ReSharper disable once ConvertIfStatementToReturnStatement
        
        // Check if move is a rank 2 killer move (less local, might've been updated long time ago).
        if (move == KillerMoveTwo) return 800000;

        // Return the updated history score for the move.
        return HistoryTable[pieceToMove, board.ColorToMove, move.To];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ScoreMoveQSearch(
        Board board, 
        ref OrderedMoveEntry move,
        SearchedMove tableMove
        )
    {
        // Compare our move with the one found from transposition table. There's no guarantee the transposition move
        // is even legal, so this acts as a sort of legal verification for it too.
        // Regardless, if our move is equal to that (also proving that it is legal for this position), then give it
        // highest priority, making it the first move we make.
        if (move == tableMove) return PRIORITY - 1;
        
        // Score promotions based on the type of promotion it is. 
        // Promotion | Score
        // Queen     | PRIORITY - 4 (HIGHEST)
        // Bishop    | PRIORITY - 5
        // Knight    | PRIORITY - 6
        // Rook      | PRIORITY - 7 (LOWEST)
        if (move.Promotion != Promotion.None) return PRIORITY - 8 + (int)move.Promotion;

        // Score captures based on the piece capturing and the piece being captured.
        // The idea behind it is to give highest priority to captures that are capturing most valuable pieces
        // with least valuable pieces.
        Piece to = board.At(move.To).Item1;
        if (to != Piece.Empty) return MvvLva(board.At(move.From).Item1, to) * 10000;

        return 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int MvvLva(Piece attacker, Piece victim) => MvvLvaTable.DJAA((int)victim, (int)attacker);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OrderedMoveList(ref Span<OrderedMoveEntry> memory, int ply, KillerMoveTable killerMoveTable, 
        HistoryTable historyTable)
    {
        Internal = memory;
        KillerMoveOne = killerMoveTable[0, ply];
        KillerMoveTwo = killerMoveTable[1, ply];
        HistoryTable = historyTable;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int NormalMoveGeneration(Board board, SearchedMove transpositionMove)
    {
        PieceColor oppositeColor = board.ColorToMove.OppositeColor();

        // Generate pins and check bitboards.
        Square kingSq = board.KingLoc(board.ColorToMove);
        (BitBoard hv, BitBoard d) = MoveList.PinBitBoards(board, kingSq, board.ColorToMove, oppositeColor);
        (BitBoard checks, bool doubleChecked) = MoveList.CheckBitBoard(board, kingSq, oppositeColor);

        // Define the list.
        int i = 0;
        BitBoardIterator fromIterator;
        Square from;
        if (!doubleChecked) {
            // We can only do this if we're not double checked.
            // In case of double-checked (discovered + normal), only the king can move so we should skip this.
            
            // Generate all pawn moves.
            fromIterator = board.All(Piece.Pawn, board.ColorToMove).GetEnumerator();
            from = fromIterator.Current;
            while (fromIterator.MoveNext()) {
                MoveList moveList = new(
                    board, from, Piece.Pawn, board.ColorToMove, 
                    ref hv, ref d, ref checks
                );
                BitBoardIterator moves = moveList.Moves.GetEnumerator();
                Square move = moves.Current;
                
                while (moves.MoveNext()) {
                    if (moveList.Promotion) {
                        int p = 1;
                        while (p < 5) {
                            Internal[i] = new OrderedMoveEntry(from, move, (Promotion)p);
                            Internal[i].Score = ScoreMoveNormal(Piece.Pawn, board, ref Internal[i], transpositionMove);
                            i++;
                            p++;
                        }
                    } else {
                        Internal[i] = new OrderedMoveEntry(from, move, Promotion.None);
                        Internal[i].Score = ScoreMoveNormal(Piece.Pawn, board, ref Internal[i], transpositionMove);
                        i++;
                    }
                    
                    move = moves.Current;
                }
                
                from = fromIterator.Current;
            }

            // Generate moves for rook, knight, bishop, and queen.
            sbyte piece = 1;
            while (piece < 5) {
                fromIterator = board.All((Piece)piece, board.ColorToMove).GetEnumerator();
                from = fromIterator.Current;
                while (fromIterator.MoveNext()) {
                    MoveList moveList = new(
                        board, from, (Piece)piece, board.ColorToMove, 
                        ref hv, ref d, ref checks
                    );
                    BitBoardIterator moves = moveList.Moves.GetEnumerator();
                    Square move = moves.Current;

                    while (moves.MoveNext()) {
                        Internal[i] = new OrderedMoveEntry(from, move, Promotion.None);
                        Internal[i].Score = ScoreMoveNormal((Piece)piece, board, ref Internal[i], transpositionMove);
                        i++;
                    
                        move = moves.Current;
                    }

                    from = fromIterator.Current;
                }

                piece++;
            }
        }
        
        // Generate all king moves.
        fromIterator = board.All(Piece.King, board.ColorToMove).GetEnumerator();
        from = fromIterator.Current;
        while (fromIterator.MoveNext()) {
            MoveList moveList = new(
                board, from, Piece.King, board.ColorToMove, 
                ref hv, ref d, ref checks
            );
            BitBoardIterator moves = moveList.Moves.GetEnumerator();
            Square move = moves.Current;

            while (moves.MoveNext()) {
                Internal[i] = new OrderedMoveEntry(from, move, Promotion.None);
                Internal[i].Score = ScoreMoveNormal(Piece.King, board, ref Internal[i], transpositionMove);
                i++;
                    
                move = moves.Current;
            }

            from = fromIterator.Current;
        }

        return i;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int QSearchMoveGeneration(Board board, SearchedMove transpositionMove)
    {
        PieceColor oppositeColor = board.ColorToMove.OppositeColor();
        // If we only want capture moves, we should also define our opposite board.
        BitBoard opposite = board.All(oppositeColor);

        // Generate pins and check bitboards.
        Square kingSq = board.KingLoc(board.ColorToMove);
        (BitBoard hv, BitBoard d) = MoveList.PinBitBoards(board, kingSq, board.ColorToMove, oppositeColor);
        (BitBoard checks, bool doubleChecked) = MoveList.CheckBitBoard(board, kingSq, oppositeColor);

        // Define the list.
        int i = 0;
        BitBoardIterator fromIterator;
        Square from;
        if (!doubleChecked) {
            // We can only do this if we're not double checked.
            // In case of double-checked (discovered + normal), only the king can move so we should skip this.
            
            // Generate all pawn moves.
            fromIterator = board.All(Piece.Pawn, board.ColorToMove).GetEnumerator();
            from = fromIterator.Current;
            while (fromIterator.MoveNext()) {
                MoveList moveList = new(board, from, ref hv, ref d, ref checks);
                moveList.LegalPawnMoveSetCapture(board.ColorToMove);
                BitBoardIterator moves = moveList.Moves.GetEnumerator();
                Square move = moves.Current;
                
                while (moves.MoveNext()) {
                    if (moveList.Promotion) {
                        int p = 1;
                        while (p < 5) {
                            Internal[i] = new OrderedMoveEntry(from, move, (Promotion)p);
                            Internal[i].Score = ScoreMoveQSearch(board, ref Internal[i], transpositionMove);
                            i++;
                            p++;
                        }
                    } else {
                        Internal[i] = new OrderedMoveEntry(from, move, Promotion.None);
                        Internal[i].Score = ScoreMoveQSearch(board, ref Internal[i], transpositionMove);
                        i++;
                    }
                    
                    move = moves.Current;
                }
                
                from = fromIterator.Current;
            }

            // Generate moves for rook, knight, bishop, and queen.
            sbyte piece = 1;
            while (piece < 5) {
                fromIterator = board.All((Piece)piece, board.ColorToMove).GetEnumerator();
                from = fromIterator.Current;
                while (fromIterator.MoveNext()) {
                    MoveList moveList = new(
                        board, from, (Piece)piece, board.ColorToMove, 
                        ref hv, ref d, ref checks
                    );
                    BitBoardIterator moves = (moveList.Moves & opposite).GetEnumerator();
                    Square move = moves.Current;

                    while (moves.MoveNext()) {
                        Internal[i] = new OrderedMoveEntry(from, move, Promotion.None);
                        Internal[i].Score = ScoreMoveQSearch(board, ref Internal[i], transpositionMove);
                        i++;
                    
                        move = moves.Current;
                    }

                    from = fromIterator.Current;
                }

                piece++;
            }
        }
        
        // Generate all king moves.
        fromIterator = board.All(Piece.King, board.ColorToMove).GetEnumerator();
        from = fromIterator.Current;
        while (fromIterator.MoveNext()) {
            MoveList moveList = new(
                board, from, Piece.King, board.ColorToMove, 
                ref hv, ref d, ref checks
            );
            BitBoardIterator moves = (moveList.Moves & opposite).GetEnumerator();
            Square move = moves.Current;

            while (moves.MoveNext()) {
                Internal[i] = new OrderedMoveEntry(from, move, Promotion.None);
                Internal[i].Score = ScoreMoveQSearch(board, ref Internal[i], transpositionMove);
                i++;
                    
                move = moves.Current;
            }

            from = fromIterator.Current;
        }

        return i;
    }

    public ref OrderedMoveEntry this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Internal[i];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SortNext(int sorted, int maxSelection)
    {
        int index = sorted;
        int i = 1 + sorted;
        while (i < maxSelection) {
            if (Internal[i].Score > Internal[index].Score) index = i;
            i++;
        }
        
        Swap(index, sorted);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Swap(int firstIndex, int secondIndex) => (Internal[firstIndex], Internal[secondIndex]) =
        (Internal[secondIndex], Internal[firstIndex]);

}