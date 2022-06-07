using System.Runtime.CompilerServices;
using Backend;
using Backend.Data.Enum;
using Backend.Data.Struct;
using Engine.Data;
using Engine.Data.Enum;
using Engine.Data.Struct;

namespace Engine;

public class MoveSearch
{

    private const int POS_INFINITY = 100000000;
    private const int NEG_INFINITY = -POS_INFINITY;
    private const int MATE = POS_INFINITY - 1;

    public int TableCutoffCount;
    public int TotalNodeSearchCount;

    private readonly Board Board;
    private readonly CancellationToken Token;
    private readonly MoveTranspositionTable Table;

    private SearchedMove BestMove;

    public MoveSearch(Board board, MoveTranspositionTable table, CancellationToken token = default)
    {
        Board = board;
        Table = table;
        BestMove = new SearchedMove(Square.Na, Square.Na, Promotion.None, NEG_INFINITY);
        Token = token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchedMove SearchAndReturn(int depth)
    {
        AbSearch(Board, 0, depth, NEG_INFINITY, POS_INFINITY);
        return BestMove;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private int AbSearch(Board board, int plyFromRoot, int depth, int alpha, int beta)
    {
        #region Cancellation

        // If we're cancelled, we should abort as soon as possible. Note, this requires a cloned Board to be
        // provided. If provided without cloning, there's no guarantee the original state will be maintained after
        // search.
        if (Token.IsCancellationRequested) throw new OperationCanceledException();

        #endregion

        int originalAlpha = alpha;
        
        #region Mate Pruning & Piece-Count Draw-Checks

        if (plyFromRoot != 0) {
            int allPiecesCount = board.All().Count;
            // If only the kings are left, it's a draw.
            if (allPiecesCount == 2) return 0;
            
            bool knightLeft =
                (bool)board.All(Piece.Knight, PieceColor.White) || board.All(Piece.Knight, PieceColor.Black);
            // If only the kings and one knight is left, it's a draw.
            if (allPiecesCount == 3 && knightLeft) return 0;
            
            bool bishopLeft = 
                (bool)board.All(Piece.Bishop, PieceColor.White) || board.All(Piece.Bishop, PieceColor.Black);
            // If only the kings and one bishop is left, it's a draw.
            if (allPiecesCount == 3 && bishopLeft) return 0;

            // If we are not at the root, we should check and see if there is a ready mate.
            // If there is, we shouldn't really care about other moves or slower mates, but instead
            // we should prune as fast as possible. It's crucial to ensuring we hit high depths.
            alpha = Math.Max(alpha, -MATE + plyFromRoot);
            beta = Math.Min(beta, MATE - plyFromRoot - 1);
            if (alpha >= beta) return alpha;
        }

        #endregion

        #region Transposition Table Lookup

        ref MoveTranspositionTableEntry storedEntry = ref Table[board.ZobristHash];
        bool valid = storedEntry.Type != MoveTranspositionTableEntryType.Invalid;
        bool transpositionUsed = false;
        if (valid && storedEntry.ZobristHash == board.ZobristHash && storedEntry.Depth >= depth && plyFromRoot != 0) {
            switch (storedEntry.Type) {
                case MoveTranspositionTableEntryType.Exact:
                    return storedEntry.BestMove.Score;
                case MoveTranspositionTableEntryType.BetaCutoff:
                    alpha = Math.Max(alpha, storedEntry.BestMove.Score);
                    break;
                case MoveTranspositionTableEntryType.AlphaUnchanged:
                    beta = Math.Min(beta, storedEntry.BestMove.Score);
                    break;
                case MoveTranspositionTableEntryType.Invalid:
                default:
                    break;
            }
        
            if (alpha >= beta) {
#if DEBUG
                TableCutoffCount++;
#endif
                return storedEntry.BestMove.Score;
            }

            transpositionUsed = true;
        }
        
        #endregion

        #region Move List Creation

        // Allocate memory on the stack to be used for our move-list.
        Span<OrderedMoveEntry> moveSpan = stackalloc OrderedMoveEntry[128];
        OrderedMoveList moveList = new(board, ref moveSpan, Table, transpositionUsed);
        
        if (moveList.Count == 0) {
            // If we had no moves at this depth, we should check if our king is in check. If our king is in check, it
            // means we lost as nothing can save the king anymore. Otherwise, it's a stalemate where we can't really do
            // anything but the opponent cannot kill our king either. It isn't a beneficial position or a position
            // that's bad for us, so returning 0 is fine here.
            Square kingSq = board.KingLoc(board.WhiteTurn ? PieceColor.White : PieceColor.Black);
            PieceColor oppositeColor = board.WhiteTurn ? PieceColor.Black : PieceColor.White;
            return MoveList.UnderAttack(board, kingSq, oppositeColor) ? -MATE + plyFromRoot : 0;
        }

        #endregion

        #region Alpha Beta Negamax
        
        int bestEvaluation = NEG_INFINITY;
        int bestMoveIndex = -1;
        
        int i = 0;
        if (depth == 1) {
            // If depth is equal to 1, then we we will just be evaluating the state of the board.
            // While we could do this at depth zero, and use the same logic as all depths > 1, it is beneficial to 
            // avoid another recursive call.
            while (i < moveList.Count) {
                // We should being the move that's likely to be the best move at this depth to the top. This ensures
                // that we are searching through the likely best moves first, allowing us to return early.
                moveList.SortNext(i);
                
                // Make the move.
                OrderedMoveEntry move = moveList[i];
                RevertMove rv = board.Move(move.From, move.To, move.Promotion);
                TotalNodeSearchCount++;
        
                // Evaluate position by getting the relative evaluation and negating it. An evaluation that's good for
                // our opponent will obviously be bad for us.
                int evaluation = -Evaluation.RelativeEvaluation(board);
                
                // Undo the move.
                board.UndoMove(ref rv);

                if (evaluation > bestEvaluation) {
                    // If our evaluation was better than our current best evaluation, we should update our evaluation
                    // with the new evaluation. We should also take into account that it was our best move so far.
                    bestEvaluation = evaluation;
                    bestMoveIndex = i;
                }

                if (evaluation > alpha) {
                    // If our evaluation was better than our alpha (best unavoidable evaluation so far), then we should
                    // replace our alpha with our evaluation. 
                    alpha = evaluation;
                    
                    // If the evaluation was better than beta, it means the position was too good. Thus, there
                    // is a good chance that the opponent will avoid this path. Hence, there is currently no
                    // reason to evaluate it further.
                    if (alpha >= beta) break;
                }
            
                i++;
            }
        } else {
            // If the depth isn't equal to 1, we must search deeper by recursive calls.
            // Calculate next iteration variables before getting into the loop.
            int nextDepth = depth - 1;
            int nextPlyFromRoot = plyFromRoot + 1;
            
            while (i < moveList.Count) {
                // We should being the move that's likely to be the best move at this depth to the top. This ensures
                // that we are searching through the likely best moves first, allowing us to return early.
                moveList.SortNext(i);
                
                // Make the move.
                OrderedMoveEntry move = moveList[i];
                RevertMove rv = board.Move(move.From, move.To, move.Promotion);
                TotalNodeSearchCount++;
        
                // Evaluate position by searching deeper and negating the result. An evaluation that's good for
                // our opponent will obviously be bad for us.
                int evaluation = -AbSearch(board, nextPlyFromRoot, nextDepth, -beta, -alpha);
                
                // Undo the move.
                board.UndoMove(ref rv);
                
                if (evaluation > bestEvaluation) {
                    // If our evaluation was better than our current best evaluation, we should update our evaluation
                    // with the new evaluation. We should also take into account that it was our best move so far.
                    bestEvaluation = evaluation;
                    bestMoveIndex = i;
                }
        
                if (evaluation > alpha) {
                    // If our evaluation was better than our alpha (best unavoidable evaluation so far), then we should
                    // replace our alpha with our evaluation.
                    alpha = evaluation;
                    
                    // If the evaluation was better than beta, it means the position was too good. Thus, there
                    // is a good chance that the opponent will avoid this path. Hence, there is currently no
                    // reason to evaluate it further.
                    if (alpha >= beta) break;
                }
            
                i++;
            }
        }
        
        #endregion

        #region Transposition Table Insertion

        MoveTranspositionTableEntryType type = MoveTranspositionTableEntryType.Exact;
        if (bestEvaluation <= originalAlpha) type = MoveTranspositionTableEntryType.AlphaUnchanged;
        else if (bestEvaluation >= beta) type = MoveTranspositionTableEntryType.BetaCutoff;
        SearchedMove bestMove = new(ref moveList[bestMoveIndex], bestEvaluation);
        MoveTranspositionTableEntry entry = new(board.ZobristHash, type, bestMove, depth);
        Table.InsertEntry(board.ZobristHash, ref entry);

        #endregion
        
        // If we're at the root node, we should also consider this our best move from the search.
        if (plyFromRoot == 0) BestMove = bestMove;

        return bestEvaluation;
    }

}