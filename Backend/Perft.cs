using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend
{

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
            MoveGeneration(Board, 4, verbose: false);
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
            bool verbose = true
            )
        {
            ulong count = 0;

            PieceColor color = board.WhiteTurn ? PieceColor.White : PieceColor.Black;
            PieceColor oppositeColor = Util.OppositeColor(color);
            int nextDepth = depth - 1;
         
            BitBoard colored = board.All(color);
            if (depth < 5) {
                BitBoardIterator coloredIterator = colored.GetEnumerator();
                Square from = coloredIterator.Current;
                if (depth == 1) {
                    while (coloredIterator.MoveNext()) {
                        MoveList moveList = new(board, from, false);
                        BitBoardIterator moveListIterator = moveList.Get().GetEnumerator();
                        Square move = moveListIterator.Current;
                        while (moveListIterator.MoveNext()) {
                            RevertMove rv = board.Move(from, move);
                            
                            BitBoard kingSafety = board.KingLoc(color);
                            if (!MoveList.UnderAttack(board, kingSafety, oppositeColor)) {
                                count += 1UL;
                                if (verbose) LogNodeCount(from, move, (ulong)moveList.Count);
                            }
                            
                            board.UndoMove(ref rv);

                            move = moveListIterator.Current;
                        }

                        from = coloredIterator.Current;
                    }
                } else {
                    while (coloredIterator.MoveNext()) {
                        MoveList moveList = new(board, from, false);
                        BitBoardIterator moveListIterator = moveList.Get().GetEnumerator();
                        Square move = moveListIterator.Current;
                        while (moveListIterator.MoveNext()) {
                            RevertMove rv = board.Move(from, move);
                            
                            BitBoard kingSafety = board.KingLoc(color);
                            if (!MoveList.UnderAttack(board, kingSafety, oppositeColor)) {
                                ulong nextCount = MoveGeneration(board, nextDepth, false);
                                count += nextCount;
                                
                                if (verbose) LogNodeCount(from, move, nextCount);
                            }
                            
                            board.UndoMove(ref rv);

                            move = moveListIterator.Current;
                        }

                        from = coloredIterator.Current;
                    }
                }
            } else {
                Parallel.ForEach((Square[])colored, ParallelOptions, from =>
                {
                    MoveList moveList = new(board, from, false);
                    BitBoard moves = moveList.Get();
                    if (moves == BitBoard.Default) return;
                    
                    Board next = board.Clone();

                    BitBoardIterator iterator = moves.GetEnumerator();
                    Square move = iterator.Current;
                    while (iterator.MoveNext()) {
                        RevertMove rv = next.Move(from, move);
                            
                        BitBoard kingSafety = next.KingLoc(color);
                        if (!MoveList.UnderAttack(next, kingSafety, oppositeColor)) {
                            ulong nextCount = MoveGeneration(next, depth - 1, false);
                            Interlocked.Add(ref count, nextCount);
                                
                            if (verbose) LogNodeCount(from, move, nextCount);
                        }
                            
                        next.UndoMove(ref rv);

                        move = iterator.Current;
                    }
                });
            }
          
            return count;
        }

    }

}