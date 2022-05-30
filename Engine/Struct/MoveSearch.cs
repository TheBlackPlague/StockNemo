using Backend;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Engine.Struct;

public class MoveSearch
{

    private const int POS_INFINITY = 100000000;
    private const int NEG_INFINITY = -POS_INFINITY;
    private const int MATE = POS_INFINITY - 1;

    private readonly Board Board;

    private SearchedMove BestMove;

    public MoveSearch(Board board)
    {
        Board = board;
        BestMove = new SearchedMove(Square.Na, Square.Na, Promotion.None, NEG_INFINITY);
    }

    public SearchedMove SearchAndReturn(int depth)
    {
        AbSearch(Board, 0, depth, NEG_INFINITY, POS_INFINITY);
        return BestMove;
    }

    private int AbSearch(Board board, int plyFromRoot, int depth, int alpha, int beta)
    {
        if (depth == 0) return Evaluation.RelativeEvaluation(board);

        if (plyFromRoot != 0) {
            alpha = Math.Max(alpha, -MATE + plyFromRoot);
            beta = Math.Min(beta, MATE - plyFromRoot - 1);
            if (alpha >= beta) return alpha;
        }
        
        // Count number of nodes explored at this depth.
        int nodes = 0;
        
        // Figure out what's our color and what's opponent's color.
        PieceColor color = board.WhiteTurn ? PieceColor.White : PieceColor.Black;
        PieceColor oppositeColor = Util.OppositeColor(color);
        
        // Calculate next depth and plyFromRoot to avoid calculation in loop.
        int nextDepth = depth - 1;
        int nextPlyFromRoot = plyFromRoot + 1;
        
        // Generate pins and check bitboards.
        Square kingSq = board.KingLoc(color);
        (BitBoard hv, BitBoard d) = MoveList.PinBitBoards(board, kingSq, color, oppositeColor);
        (BitBoard checks, bool doubleChecked) = MoveList.CheckBitBoard(board, kingSq, oppositeColor);
        
        // Setup iterator to be used to go through all squares occupied by our pieces.
        BitBoardIterator coloredIterator = Board.All(color).GetEnumerator();
        Square pieceSq = coloredIterator.Current;
        
        while (coloredIterator.MoveNext()) {
            // Determine what piece is at this square. It will be our color.
            (Piece piece, _) = board.At(pieceSq);
            
            // If we're double checked (discovered + normal), then only our king will have legal moves. Thus, there is
            // no point trying to generate moves for any other piece and we can skip the iterations.
            if (doubleChecked && piece != Piece.King) {
                pieceSq = coloredIterator.Current;
                continue;
            }
            
            // Generate all legal moves for our piece using the pin and check bitboards.
            MoveList moveList = new(
                board, pieceSq, piece, color, 
                ref hv, ref d, ref checks, doubleChecked
            );
            
            // Setup iterator to be used to go through all squares our piece can move to.
            BitBoardIterator moveIterator = moveList.Moves.GetEnumerator();
            Square move = moveIterator.Current;

            while (moveIterator.MoveNext()) {
                // Make the move.
                RevertMove rv = board.Move(pieceSq, move);
                    
                if (moveList.Promotion) {
                    // In the case of a promotion, we must do the promotion to evaluate position after the promotion.
                    // There are 4 possible promotions (moves), so we must check all of them.
                    nodes += 4;
                    int i = 1;
                    while (i < 5) {
                        // Undo the original pawn move/previous iteration promotion move.
                        board.UndoMove(ref rv);
                        Promotion promotion = (Promotion)i;
                        
                        // Make the promotion move.
                        rv = board.Move(pieceSq, move, promotion);
                        
                        // Evaluate position by searching deeper.
                        int evaluation = -AbSearch(board, nextPlyFromRoot, nextDepth, -beta, -alpha);
                        if (evaluation >= beta) {
                            // If the evaluation was better than beta, it means the position was too good. Thus, there
                            // is a good chance that the opponent will avoid this path. Hence, there is currently no
                            // reason to evaluate it further.
                            board.UndoMove(ref rv);
                            return beta;
                        }

                        if (evaluation > alpha) {
                            // If our evaluation was better than our alpha (best evaluation so far), then we should
                            // replace our alpha with our evaluation.
                            alpha = evaluation;
                            
                            // If we're at the original state, we should be setting the moves correctly.
                            if (plyFromRoot == 0) BestMove = new SearchedMove(pieceSq, move, promotion, alpha);
                        }

                        i++;
                    }
                } else {
                    // If there was no promotion, then there is only one move.
                    nodes++;
                    
                    // Evaluate position by searching deeper.
                    int evaluation = -AbSearch(board, nextPlyFromRoot, nextDepth, -beta, -alpha);
                    if (evaluation >= beta) {
                        // If the evaluation was better than beta, it means the position was too good. Thus, there
                        // is a good chance that the opponent will avoid this path. Hence, there is currently no
                        // reason to evaluate it further.
                        board.UndoMove(ref rv);
                        return beta;
                    }

                    if (evaluation > alpha) {
                        // If our evaluation was better than our alpha (best evaluation so far), then we should
                        // replace our alpha with our evaluation.
                        alpha = evaluation;
                        
                        // If we're at the original state, we should be setting the moves correctly.
                        if (plyFromRoot == 0) BestMove = new SearchedMove(pieceSq, move, Promotion.None, alpha);
                    }
                }
                    
                // Undo the move made to get back to original state.
                board.UndoMove(ref rv);

                move = moveIterator.Current;
            }
            
            pieceSq = coloredIterator.Current;
        }

        if (nodes == 0) {
            // If we had no moves at this depth, we should check if our king is in check.
            // If our king is in check, it means we lost as nothing can save the king anymore.
            // Otherwise, it's a stalemate where we can't really do anything but the opponent cannot
            // kill our king either.
            return MoveList.UnderAttack(board, kingSq, oppositeColor) ? -MATE + plyFromRoot : 0;
        }

        // Return the best evaluation we've had this depth.
        return alpha;
    }

}