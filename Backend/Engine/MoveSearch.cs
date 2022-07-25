using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Backend.Data;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend.Engine;

public class MoveSearch
{

    private const int POS_INFINITY = 100000000;
    private const int NEG_INFINITY = -POS_INFINITY;
    private const int MATE = POS_INFINITY - 1;

    private const int NULL_MOVE_REDUCTION = 3;
    private const int NULL_MOVE_DEPTH = NULL_MOVE_REDUCTION - 1;

    private const int ASPIRATION_BOUND = 3500;
    private const int ASPIRATION_DELTA = 30;
    private const int ASPIRATION_DEPTH = 4;

    private const int RAZORING_EVALUATION_THRESHOLD = 150;

    private const int LMR_FULL_SEARCH_THRESHOLD = 4;
    private const int LMR_DEPTH_THRESHOLD = 3;

    private const int NODE_COUNTING_DEPTH = 8;
    private const int NODE_COUNTING_REQUIRED_EFFORT = 95;

    private const int REVERSE_FUTILITY_D = 70;
    private const int REVERSE_FUTILITY_I = 80;
    private const int REVERSE_FUTILITY_DEPTH_THRESHOLD = 8;

    private const float TIME_TO_DEPTH_THRESHOLD = 0.2f;

    public int TableCutoffCount { get; private set; }
    public int TotalNodeSearchCount { get; private set; }

    private static readonly LogarithmicReductionDepthTable ReductionDepthTable = new();

    private readonly HistoryTable HistoryTable = new();
    private readonly KillerMoveTable KillerMoveTable = new();
    private readonly MoveSearchEffortTable SearchEffort = new();
    private readonly PrincipleVariationTable PvTable = new();

    private readonly int[] PositionalEvaluationStore = new int[128];

    private readonly EngineBoard Board;
    private readonly TimeControl TimeControl;
    private readonly MoveTranspositionTable Table;

    private OrderedMoveEntry ReducedTimeMove = OrderedMoveEntry.Default;

    public MoveSearch(EngineBoard board, MoveTranspositionTable table, TimeControl timeControl)
    {
        Board = board;
        Table = table;
        TimeControl = timeControl;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public OrderedMoveEntry IterativeDeepening(int selectedDepth)
    {
        OrderedMoveEntry bestMove = OrderedMoveEntry.Default;
        int evaluation = NEG_INFINITY;
        try {
            int depth = 1;
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool timePreviouslyUpdated = false;
            while (!TimeControl.Finished() && depth <= selectedDepth) {
                evaluation = AspirationSearch(Board, depth, evaluation);
                bestMove = PvTable.Get(0);

                // Try counting nodes to see if we can exit the search early.
                timePreviouslyUpdated = NodeCounting(depth, bestMove, timePreviouslyUpdated);
                
                DepthSearchLog(depth, evaluation, stopwatch);
                
                // In the case we are past a certain depth, and are really low on time, it's highly unlikely we'll
                // finish the next depth in time. To save time, we should just exit the search early.
                if (depth > 5 && TimeControl.TimeLeft() <= TimeControl.Time * TIME_TO_DEPTH_THRESHOLD) break;
                
                depth++;
            }
        } catch (OperationCanceledException) {}
        return bestMove;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private int AspirationSearch(EngineBoard board, int depth, int previousEvaluation)
    {
        // Set base window size.
        int alpha = NEG_INFINITY;
        int beta = POS_INFINITY;

        if (depth > ASPIRATION_DEPTH) {
            // If we're searching deeper than our aspiration depth, then we should modify the window based on our
            // previous evaluation. If the window isn't reasonably correct, it'll get reset later anyways.
            alpha = previousEvaluation - 50;
            beta = previousEvaluation + 50;
        }

        int research = 0;
        while (true) {
            #region Out of Time

            // If we're out of time, we should exit the search as fast as possible.
            // NOTE: Due to the nature of this exit (using exceptions to do it as fast as possible), the board state
            // is not reverted. Thus, a cloned board must be provided.
            if (TimeControl.Finished()) throw new OperationCanceledException();

            #endregion

            #region Reset Window
            
            // We should reset our window if it's too far gone because the gradual increase isn't working.

            // In the case our alpha is far below our aspiration bound, we should reset it to negative infinity for
            // our research.
            if (alpha < -ASPIRATION_BOUND) alpha = NEG_INFINITY;
            
            // In the case our beta is far too above our aspiration bound, we should reset it to positive infinity for
            // our research.
            if (beta > ASPIRATION_BOUND) beta = POS_INFINITY;

            #endregion
            
            // Get our best evaluation so far so we can decide whether we need to do a research or not.
            // Researches are reasonably fast thanks to transposition tables.
            int bestEvaluation = AbSearch(board, 0, depth, alpha, beta);

            #region Modify Window

            if (bestEvaluation <= alpha) {
                research++;
                
                // If our best evaluation was somehow worse than our alpha, we should resize our window and research.
                alpha = Math.Max(alpha - research * research * ASPIRATION_DELTA, NEG_INFINITY);
            } else if (bestEvaluation >= beta) {
                research++;
                
                // If our evaluation was somehow better than our beta, we should resize our window and research.
                beta = Math.Min(beta + research * research * ASPIRATION_DELTA, POS_INFINITY);
                
                // If our evaluation was within our window, we should return the result avoiding any researches.
            } else return bestEvaluation;

            #endregion
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private int AbSearch(EngineBoard board, int plyFromRoot, int depth, int alpha, int beta)
    {
        #region Out of Time

        // If we're out of time, we should exit the search as fast as possible.
        // NOTE: Due to the nature of this exit (using exceptions to do it as fast as possible), the board state
        // is not reverted. Thus, a cloned board must be provided.
        if (TimeControl.Finished()) throw new OperationCanceledException();

        #endregion

        #region Pv Table Length Initialization

        PvTable.InitializeLength(plyFromRoot);

        #endregion
        
        #region QSearch Jump

        // At depth 0 (or less in the case of reductions etc.), since we may be having a capture train, we should jump
        // into QSearch and evaluate even deeper. In the case of no captures available, QSearch will throw us out
        // instantly.
        if (depth <= 0) return QSearch(board, plyFromRoot, 15, alpha, beta);
        
        #endregion
        
        bool rootNode = plyFromRoot == 0;
        bool notRootNode = !rootNode;

        #region Mate Pruning & Piece-Count Draw-Checks

        if (notRootNode) {
            // We had a three-fold repetition, so return earlier.
            if (board.IsRepetition()) return 0;
            
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
        SearchedMove transpositionMove = SearchedMove.Default;
        bool transpositionHit = false;

        if (valid && storedEntry.ZobristHash == board.ZobristHash) {
            // We had a transposition table hit. However, at this point, we don't know if this is a trustworthy
            // transposition hit or not.
            transpositionMove = storedEntry.BestMove;
            transpositionHit = true;

            if (storedEntry.Depth >= depth && plyFromRoot != 0) {
                // If it came from a higher depth search than our current depth, it means the results are definitely
                // more trustworthy than the ones we could achieve at this depth.
                switch (storedEntry.Type) {
                    case MoveTranspositionTableEntryType.Exact:
                        // In the case of an exact evaluation, we have previously found this was our best move
                        // in said transposition. Therefore, it is reasonable to return early.
                        return storedEntry.BestMove.Evaluation;
                    
                    // In the case that we didn't have an exact, we must alter our bounds to make our search for this
                    // depth as best as possible (and possibly get a cutoff without having to search).
                    case MoveTranspositionTableEntryType.BetaCutoff:
                        // In the case we had a beta-cutoff, we can check the max between our alpha and the stored 
                        // beta-cutoff and set it as our new alpha. This is to ensure all moves will be better than the
                        // stored cutoff.
                        alpha = Math.Max(alpha, storedEntry.BestMove.Evaluation);
                        break;
                    case MoveTranspositionTableEntryType.AlphaUnchanged:
                        // In the rare case that alpha was unchanged, we must try and change the beta value to
                        // be the minimum value between our current beta and the stored unchanged alpha. This ensures
                        // that if alpha would remain unchanged, we would receive a beta-cutoff.
                        beta = Math.Min(beta, storedEntry.BestMove.Evaluation);
                        break;
                    case MoveTranspositionTableEntryType.Invalid:
                    default:
                        break;
                }

                if (alpha >= beta) {
#if DEBUG
                    TableCutoffCount++;
#endif
                    // In the case that our alpha was equal or greater than our beta, we should return the stored
                    // evaluation earlier because it was the best one possible at this transposition. Otherwise,
                    // we are required to search deeper.
                    // This happens because we edited bounds earlier.
                    return storedEntry.BestMove.Evaluation;
                }
            }
        }
        
        #endregion
        
        // Calculate deeper ply.
        int nextPlyFromRoot = plyFromRoot + 1;

        // Determine whether we should prune moves.
        PieceColor oppositeColor = Util.OppositeColor(board.ColorToMove);
        Square kingSq = board.KingLoc(board.ColorToMove);
        bool inCheck = MoveList.UnderAttack(board, kingSq, oppositeColor);
        bool improving = false;
        
        if (!inCheck) {
            // We should use the evaluation from our transposition table if we had a hit.
            // As that evaluation isn't truly static and may have been from a previous deep search.
            int positionalEvaluation = transpositionHit ? 
                transpositionMove.Evaluation : Evaluation.RelativeEvaluation(board);
            
            // Also store the evaluation to later check if it improved.
            PositionalEvaluationStore.AA(plyFromRoot) = positionalEvaluation;
            
            // Roughly estimate whether the deeper search improves the position or not.
            improving = plyFromRoot >= 2 && positionalEvaluation >= PositionalEvaluationStore.AA(plyFromRoot - 2);

            #region Reverse Futility Pruning

            if (depth < REVERSE_FUTILITY_DEPTH_THRESHOLD && Math.Abs(beta) < MATE &&
                positionalEvaluation - REVERSE_FUTILITY_D * depth + REVERSE_FUTILITY_I * improving.ToByte() >= beta) 
                return beta;

            #endregion
            
            #region Razoring
            
            if (depth == 1 && positionalEvaluation + RAZORING_EVALUATION_THRESHOLD < alpha)
                // If after any move, the positional evaluation of the resulting position with some added threshold is
                // less than alpha, then the opponent will be able to find at least one move that improves their
                // position.
                // Thus, we can avoid trying moves and jump into QSearch to get exact evaluation of the position.
                return QSearch(board, plyFromRoot, 15, alpha, beta);
            
            #endregion
            
            #region Null Move Pruning

            // Reduction depth for null move pruning.
            int reductionDepth = depth - NULL_MOVE_REDUCTION;
        
            if (notRootNode && depth > NULL_MOVE_DEPTH) {
                // For null move pruning, we give the turn to the opponent and let them make the move.
                RevertNullMove rv = board.NullMove();
                // Then we evaluate position by searching at a reduced depth using same characteristics as normal search.
                // The idea is that if there are cutoffs, most will be found using this reduced search and we can cutoff
                // this branch earlier.
                // Being reduced, it's not as expensive as the regular search (especially if we can avoid a jump into
                // QSearch).
                int evaluation = -AbSearch(board, nextPlyFromRoot, reductionDepth, -beta, -beta + 1);
                // Undo the null move so we can get back to original state of the board.
                board.UndoNullMove(rv);
        
                // In the case our evaluation was better than our beta, we achieved a cutoff here. 
                if (evaluation >= beta) return beta;
            }

            #endregion
        }

        #region Move List Creation

        // Allocate memory on the stack to be used for our move-list.
        Span<OrderedMoveEntry> moveSpan = stackalloc OrderedMoveEntry[OrderedMoveList.SIZE];
        OrderedMoveList moveList = new(ref moveSpan, plyFromRoot, KillerMoveTable, HistoryTable);
        int moveCount = moveList.NormalMoveGeneration(board, transpositionMove);
        
        if (moveCount == 0) {
            // If we had no moves at this depth, we should check if our king is in check. If our king is in check, it
            // means we lost as nothing can save the king anymore. Otherwise, it's a stalemate where we can't really do
            // anything but the opponent cannot kill our king either. It isn't a beneficial position or a position
            // that's bad for us, so returning 0 is fine here.
            return inCheck ? -MATE + plyFromRoot : 0;
        }

        #endregion

        #region Fail-soft Alpha Beta Negamax
        
        int bestEvaluation = NEG_INFINITY;
        OrderedMoveEntry bestMoveSoFar = new(Square.Na, Square.Na, Promotion.None);
        MoveTranspositionTableEntryType transpositionTableEntryType = MoveTranspositionTableEntryType.AlphaUnchanged;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool HandleEvaluation(int evaluation, ref OrderedMoveEntry move, bool quietMove)
        {
            if (evaluation <= bestEvaluation) return true;
            
            // If our evaluation was better than our current best evaluation, we should update our evaluation
            // with the new evaluation. We should also take into account that it was our best move so far.
            bestEvaluation = evaluation;
            bestMoveSoFar = move;
            
            // Insert move into PV Table.
            PvTable.Insert(plyFromRoot, ref move);
            
            // Copy moves from lesser ply to current ply PV Line.
            int nextPly = plyFromRoot + 1;
            while (PvTable.PlyInitialized(plyFromRoot, nextPly)) {
                PvTable.Copy(plyFromRoot, nextPly);
                nextPly++;
            }
            
            // Update our PV Length.
            PvTable.UpdateLength(plyFromRoot);

            if (evaluation <= alpha) return true;

            // If our evaluation was better than our alpha (best unavoidable evaluation so far), then we should
            // replace our alpha with our evaluation.
            alpha = evaluation;
            
            // Update our history table with our alpha-changing quiet move in hopes we can find similar best
            // moves faster.
            if (quietMove) HistoryTable[board.PieceOnly(move.From), board.ColorToMove, move.To] += depth;
            
            // Our alpha changed, so it is no longer an unchanged alpha entry.
            transpositionTableEntryType = MoveTranspositionTableEntryType.Exact;
            
            // If the evaluation was better than beta, it means the position was too good. Thus, there
            // is a good chance that the opponent will avoid this path. Hence, there is currently no
            // reason to evaluate it further.
            return evaluation < beta;
        }
        
        // Calculate next iteration variables before getting into the loop.
        int nextDepth = depth - 1;
            
        int i = 0;
        while (i < moveCount) {
            // We should being the move that's likely to be the best move at this depth to the top. This ensures
            // that we are searching through the likely best moves first, allowing us to return early.
            moveList.SortNext(i, moveCount);

            int previousNodeCount = TotalNodeSearchCount;
            
            OrderedMoveEntry move = moveList[i];

            bool quietMove = !board.All(oppositeColor)[move.To];

            // Make the move.
            RevertMove rv = board.Move(ref move);
            TotalNodeSearchCount++;

            #region Late Move Reduction
            
            int evaluation;
            
            if (i >= LMR_FULL_SEARCH_THRESHOLD && depth >= LMR_DEPTH_THRESHOLD && !inCheck) {
                // If we're past the threshold where should search each move fully, not in any immediate danger by
                // opponent, and above the depth threshold (as to avoid inaccurate evaluations), we should reduce how
                // deep we're searching.
                
                // Evaluate an initial reduced depth depending on the number of moves played and the depth currently
                // being searched.
                int reducedDepth = ReductionDepthTable[depth, i];
                
                // Evaluate position by searching deeper and negating the result. An evaluation that's good for
                // our opponent will obviously be bad for us.
                evaluation = -AbSearch(board, nextPlyFromRoot, depth - reducedDepth, -alpha - 1, -alpha);
                
                // In the case we couldn't apply LMR, we just set our evaluation to a value greater than alpha to force
                // a full depth search.
            } else evaluation = alpha + 1;

            // In the case that we cannot do LMR (being unsafe at this depth or for this move) or LMR fails, we should
            // do a full depth search. Thanks to transposition tables, the full depth search is reasonably fast.
            if (evaluation > alpha) 
                // Evaluate position by searching deeper and negating the result. An evaluation that's good for
                // our opponent will obviously be bad for us.
                evaluation = -AbSearch(board, nextPlyFromRoot, nextDepth, -beta, -alpha);

            #endregion
                
            // Undo the move.
            board.UndoMove(ref rv);

            if (!HandleEvaluation(evaluation, ref move, quietMove)) {
                if (quietMove && KillerMoveTable[0, plyFromRoot] != move) {
                    // Given this move isn't a capture move (quiet move), we store it as a killer move (cutoff move) to
                    // better sort quiet moves like these in the future, allowing us to achieve a cutoff faster. Also
                    // make sure we are not saving same move in both of our caches.
                    KillerMoveTable.ReOrder(plyFromRoot);
                    KillerMoveTable[0, plyFromRoot] = move;
                }

                // We had a beta cutoff, hence it's a beta cutoff entry.
                transpositionTableEntryType = MoveTranspositionTableEntryType.BetaCutoff;
                break;
            }

            if (rootNode) SearchEffort[move.From, move.To] = TotalNodeSearchCount - previousNodeCount;
            
            i++;
        }
        
        #endregion

        #region Transposition Table Insertion
        
        SearchedMove bestMove = new(ref bestMoveSoFar, bestEvaluation);
        MoveTranspositionTableEntry entry = new(board.ZobristHash, transpositionTableEntryType, bestMove, depth);
        Table.InsertEntry(board.ZobristHash, ref entry);

        #endregion

        return bestEvaluation;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private int QSearch(EngineBoard board, int plyFromRoot, int depth, int alpha, int beta)
    {
        #region Out of Time

        // If we're out of time, we should exit the search as fast as possible.
        // NOTE: Due to the nature of this exit (using exceptions to do it as fast as possible), the board state
        // is not reverted. Thus, a cloned board must be provided.
        if (TimeControl.Finished()) throw new OperationCanceledException();

        #endregion
        
        #region Early Evaluation
        
        int earlyEval = Evaluation.RelativeEvaluation(board);
        
        // In the rare case our evaluation is already too good, we don't need to further evaluate captures any further,
        // as this position is overwhelmingly winning.
        if (earlyEval >= beta) return beta;
        
        // In the case that our current evaluation is better than our alpha, we need to recalibrate alpha to make sure
        // we don't skip over our already good move.
        if (earlyEval > alpha) alpha = earlyEval;
        
        #endregion
        
        #region Move List Creation

        // Allocate memory on the stack to be used for our move-list.
        Span<OrderedMoveEntry> moveSpan = stackalloc OrderedMoveEntry[OrderedMoveList.SIZE];
        OrderedMoveList moveList = new(ref moveSpan, plyFromRoot, KillerMoveTable, HistoryTable);
        int moveCount = moveList.QSearchMoveGeneration(board, SearchedMove.Default);
        
        // if (moveCount == 0) {
        //     // If we had no moves at this depth, we should check if our king is in check. If our king is in check, it
        //     // means we lost as nothing can save the king anymore. Otherwise, we should just return our best evaluation
        //     // so far.
        //     PieceColor oppositeColor = Util.OppositeColor(board.ColorToMove);
        //     Square kingSq = board.KingLoc(board.ColorToMove);
        //     return MoveList.UnderAttack(board, kingSq, oppositeColor) ? -MATE + plyFromRoot : alpha;
        // }

        #endregion
        
        #region Fail-soft Alpha Beta Negamax
        
        int bestEvaluation = earlyEval;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool HandleEvaluation(int evaluation)
        {
            if (evaluation <= bestEvaluation) return true;
            
            // If our evaluation was better than our current best evaluation, we should update our evaluation
            // with the new evaluation.
            bestEvaluation = evaluation;

            if (evaluation <= alpha) return true;

            // If our evaluation was better than our alpha (best unavoidable evaluation so far), then we should
            // replace our alpha with our evaluation.
            alpha = evaluation;
            
            // If the evaluation was better than beta, it means the position was too good. Thus, there
            // is a good chance that the opponent will avoid this path. Hence, there is currently no
            // reason to evaluate it further.
            return evaluation < beta;
        }

        // Calculate next iteration variables before getting into the loop.
        int nextDepth = depth - 1;
        int nextPlyFromRoot = plyFromRoot + 1;
            
        int i = 0;
        while (i < moveCount) {
            // We should being the move that's likely to be the best move at this depth to the top. This ensures
            // that we are searching through the likely best moves first, allowing us to return early.
            moveList.SortNext(i, moveCount);
                
            // Make the move.
            OrderedMoveEntry move = moveList[i];
            RevertMove rv = board.Move(ref move);
            TotalNodeSearchCount++;
        
            // Evaluate position by searching deeper and negating the result. An evaluation that's good for
            // our opponent will obviously be bad for us.
            int evaluation = -QSearch(board, nextPlyFromRoot, nextDepth, -beta, -alpha);
                
            // Undo the move.
            board.UndoMove(ref rv);

            if (!HandleEvaluation(evaluation)) break;
            
            i++;
        }
        
        #endregion
        
        return bestEvaluation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DepthSearchLog(int depth, int evaluation, Stopwatch stopwatch)
    {
        Console.Write(
            "info depth " + depth + " score cp " + evaluation + " nodes " + 
            TotalNodeSearchCount + " nps " + (int)(TotalNodeSearchCount / ((float)stopwatch.ElapsedMilliseconds / 1000)) 
            + " pv " + PvLine() + '\n'
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool NodeCounting(int depth, OrderedMoveEntry bestMove, bool timePreviouslyUpdated)
    {
        // This idea is from the Koivisto Engine:
        // The branch being searched the most is likely the best branch as we're having to evaluate it very deeply
        // across depths. Thus it's reasonable to end the search earlier and make the move instantly.

        // Check whether we're past the depth to start reducing our search time with node counting and make sure that
        // we're past the required effort threshold to do this move quickly.
        if (depth >= NODE_COUNTING_DEPTH && TimeControl.TimeLeft() != 0 && !timePreviouslyUpdated
            && SearchEffort[bestMove.From, bestMove.To] * 100 / TotalNodeSearchCount >= NODE_COUNTING_REQUIRED_EFFORT) {
            timePreviouslyUpdated = true;
            TimeControl.ChangeTime(TimeControl.Time / 3);
            ReducedTimeMove = bestMove;
        }

        if (timePreviouslyUpdated && bestMove != ReducedTimeMove) {
            // In the rare case that our previous node count guess was incorrect, give us a little bit more time
            // to see if we can find a better move.
            TimeControl.ChangeTime(TimeControl.Time * 3);
        }

        return timePreviouslyUpdated;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string PvLine()
    {
        StringBuilder pv = new();
        int count = PvTable.Count();
        
        for (int i = 0; i < count; i++) {
            ref OrderedMoveEntry move = ref PvTable.Get(i);
            
            pv.Append(move.From).Append(move.To);
            if (move.Promotion != Promotion.None) pv.Append(move.Promotion.ToUciNotation());
            pv.Append(' ');
        }
        
        return pv.ToString().ToLower();
    }

}