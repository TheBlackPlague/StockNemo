using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend;

public class Perft
{
        
    private const ulong D1 = 20;
    private const ulong D2 = 400;
    private const ulong D3 = 8902;
    private const ulong D4 = 197281;
    private const ulong D5 = 4865609;
    private const ulong D6 = 119060324;
    private const ulong D7 = 3195901860;

    private static readonly ParallelOptions ParallelOptions = new()
    {
        MaxDegreeOfParallelism = 4
    };
        
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private Board Board = Board.Default();

    private static void LogNodeCount(Square piece, Square move, ulong nodeC)
    {
        string fullMove = piece.ToString() + move;
        Console.WriteLine(fullMove.ToLower() + ": " + nodeC);
    }

    public Perft()
    {
        // Draw the board being tested.
        Console.WriteLine(Board.ToString());
            
        // Involve JIT.
        MoveGeneration(Board, 4, divide: false);
    }
        
    public (ulong, ulong) Depth1()
    {
        return (D1, MoveGeneration(Board, 1));
    }
        
    public (ulong, ulong) Depth2()
    {
        return (D2, MoveGeneration(Board, 2));
    }

    public (ulong, ulong) Depth3()
    {
        return (D3, MoveGeneration(Board, 3));
    }

    public (ulong, ulong) Depth4()
    {
        return (D4, MoveGeneration(Board, 4));
    }
        
    public (ulong, ulong) Depth5()
    {
        return (D5, MoveGeneration(Board, 5));
    }
        
    public (ulong, ulong) Depth6()
    {
        return (D6, MoveGeneration(Board, 6));
    }
        
    public (ulong, ulong) Depth7()
    {
        return (D7, MoveGeneration(Board, 7));
    }
        
    public static ulong MoveGeneration(
        Board board, 
        int depth, 
        bool divide = true
    )
    {
        // Store the count in a uint64.
        ulong count = 0;

        // Figure out color and opposite color from the one set in the board.
        PieceColor color = board.WhiteTurn ? PieceColor.White : PieceColor.Black;
        PieceColor oppositeColor = Util.OppositeColor(color);

        // Get all squares occupied by our color.
        BitBoard colored = board.All(color);
        if (depth < 5) {
            // If depth is less than 5 (1 - 4), we should do PERFT synchronously as it's fast enough that the cost
            // of pushing to other threads actually slows it down.
            BitBoardIterator coloredIterator = colored.GetEnumerator();
            Square from = coloredIterator.Current;
            if (depth == 1) {
                // If depth is 1, then we don't need to do any further recursion and can just do +1 to the count.
                while (coloredIterator.MoveNext()) {
                    // Generate all pseudo-legal moves for our square iteration.
                    MoveList moveList = MoveList.WithoutProvidedPins(board, from);
                    BitBoardIterator moveListIterator = moveList.Moves.GetEnumerator();
                    Square move = moveListIterator.Current;
                        
                    while (moveListIterator.MoveNext()) {
                        // Make our move iteration for our square iteration. Save the revert move for reverting
                        // in future.
                        RevertMove rv = board.Move(from, move);

                        BitBoard kingSafety = board.KingLoc(color);
                        if (!MoveList.UnderAttack(board, kingSafety, oppositeColor)) {
                            // If our king is safe, that move is legal and can be added to the count.
                            // In case of promotion, there are 4 possible outcomes.
                            count += moveList.Promotion ? 4UL : 1UL;
                                
                            // If we're dividing at this depth, log the move with the count.
                            if (divide) LogNodeCount(from, move, moveList.Promotion ? 4UL : 1UL);
                        }
                            
                        // Revert the move to get back to original state.
                        board.UndoMove(ref rv);

                        move = moveListIterator.Current;
                    }

                    from = coloredIterator.Current;
                }
            } else {
                // If depth is > 1, then we need to do recursion at depth = depth - 1.
                // Pre-figure our next depth to avoid calculations inside loop.
                int nextDepth = depth - 1;
                    
                while (coloredIterator.MoveNext()) {
                    // Generate all pseudo-legal moves for our square iteration.
                    MoveList moveList = MoveList.WithoutProvidedPins(board, from);
                    BitBoardIterator moveListIterator = moveList.Moves.GetEnumerator();
                    Square move = moveListIterator.Current;
                        
                    while (moveListIterator.MoveNext()) {
                        // Make our move iteration for our square iteration. Save the revert move for reverting
                        // in future.
                        RevertMove rv = board.Move(from, move);
                            
                        BitBoard kingSafety = board.KingLoc(color);
                        if (!MoveList.UnderAttack(board, kingSafety, oppositeColor)) {
                            // If our king is safe, that move is legal and we can calculate moves at lesser
                            // depth recursively, but we shouldn't divide at lesser depth.
                            ulong nextCount;
                            if (moveList.Promotion) {
                                // Undo original pawn move without promotion.
                                board.UndoMove(ref rv);
                                
                                // Promote to rook.
                                rv = board.Move(from, move, Promotion.Rook);
                                nextCount = MoveGeneration(board, nextDepth, false);
                                board.UndoMove(ref rv);

                                // Promote to knight.
                                rv = board.Move(from, move, Promotion.Knight);
                                nextCount += MoveGeneration(board, nextDepth, false);
                                board.UndoMove(ref rv);
                                
                                // Promote to bishop.
                                rv = board.Move(from, move, Promotion.Bishop);
                                nextCount += MoveGeneration(board, nextDepth, false);
                                board.UndoMove(ref rv);
                                
                                // Promote to queen.
                                rv = board.Move(from, move, Promotion.Queen);
                                nextCount += MoveGeneration(board, nextDepth, false);
                                
                                // Don't undo the final move as it's done outside.
                            } else nextCount = MoveGeneration(board, nextDepth, false);
                                
                            // Add the number of moves calculated at the lesser depth.
                            count += nextCount;
                                
                            // If we're dividing at this depth, log the move with the count.
                            if (divide) LogNodeCount(from, move, nextCount);
                        }
                            
                        // Revert the move to get back to original state.
                        board.UndoMove(ref rv);

                        move = moveListIterator.Current;
                    }

                    from = coloredIterator.Current;
                }
            }
        } else {
            // If depth is more than 4, we can achieve significant performance increase by running recursive
            // iterations in parallel.
            Parallel.ForEach((Square[])colored, ParallelOptions, from =>
            {
                // Generate all pseudo-legal moves for our square iteration.
                MoveList moveList = MoveList.WithoutProvidedPins(board, from);
                BitBoard moves = moveList.Moves;
                    
                // If there are no legal moves, we don't need to waste further resources on this iteration.
                if (moves == BitBoard.Default) return;
                    
                // Clone the board to allow memory-safe operations: ensures that there are no overrides from
                // other threads.
                Board next = board.Clone();

                BitBoardIterator iterator = moves.GetEnumerator();
                Square move = iterator.Current;

                int nextDepth = depth - 1;
                    
                while (iterator.MoveNext()) {
                    // Make our move iteration for our square iteration. Save the revert move for reverting
                    // in future.
                    RevertMove rv = next.Move(from, move);
                            
                    BitBoard kingSafety = next.KingLoc(color);
                    if (!MoveList.UnderAttack(next, kingSafety, oppositeColor)) {
                        // If our king is safe, that move is legal and we can calculate moves at lesser
                        // depth recursively, but we shouldn't divide at lesser depth.
                        
                        ulong nextCount;
                        
                        if (moveList.Promotion) {
                            // Undo original pawn move without promotion.
                            next.UndoMove(ref rv);
                                
                            // Promote to rook.
                            rv = next.Move(from, move, Promotion.Rook);
                            nextCount = MoveGeneration(next, nextDepth, false);
                            next.UndoMove(ref rv);

                            // Promote to knight.
                            rv = next.Move(from, move, Promotion.Knight);
                            nextCount += MoveGeneration(next, nextDepth, false);
                            next.UndoMove(ref rv);
                                
                            // Promote to bishop.
                            rv = next.Move(from, move, Promotion.Bishop);
                            nextCount += MoveGeneration(next, nextDepth, false);
                            next.UndoMove(ref rv);
                                
                            // Promote to queen.
                            rv = next.Move(from, move, Promotion.Queen);
                            nextCount += MoveGeneration(next, nextDepth, false);
                                
                            // Don't undo the final move as it's done outside.
                        } else nextCount = MoveGeneration(next, nextDepth, false);
                            
                        // Add the number of moves calculated at the lesser depth. Since we're going to be adding
                        // from multiple threads, it's possible that race-conditions occur. That's why we have to
                        // lock all threads while the addition is happening.
                        Interlocked.Add(ref count, nextCount);
                                
                        // If we're dividing at this depth, log the move with the count.
                        if (divide) LogNodeCount(from, move, nextCount);
                    }
                            
                    // Revert the move to get back to original state.
                    next.UndoMove(ref rv);

                    move = iterator.Current;
                }
            });
        }
          
        return count;
    }

}