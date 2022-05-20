using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend
{

    public class Perft
    {

        private const ulong D0 = 1;
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
        
        private static void LogNodeCount(Square piece, int nodeC)
        {
            Console.WriteLine(piece.ToString().ToLower() + ": " + nodeC);
        }

        public Perft()
        {
            // Draw the board being tested.
            Console.WriteLine(Board.ToString());
            
            // Involve JIT.
            MoveGeneration(Board, 4, verbose: false);
        }
        
        public (ulong, ulong) Depth0()
        {
            return (D0, MoveGeneration(Board, 0));
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
            PieceColor color = PieceColor.White, 
            bool verbose = true
            )
        {
            if (depth == 0) return 1;
            ulong count = 0;
            
            PieceColor oppositeColor = Util.OppositeColor(color);
            int nextDepth = depth - 1;
         
            BitBoard colored = board.All(color);
            if (depth < 5) {
                foreach (Square from in colored) {
                    MoveList moveList = new(board, from);
                    if (depth == 1) {
                        count += (ulong)moveList.Count;

                        if (verbose) LogNodeCount(from, moveList.Count);
                    } else {
                        if (moveList.Count == 0) continue;

                        BitBoardMap originalState = board.GetCurrentState;
                        foreach (Square move in moveList.Get()) {
                            board.Move(from, move);
                            ulong nextCount = MoveGeneration(board, nextDepth, oppositeColor, false);
                            count += nextCount;
                            board.UndoMove(ref originalState);
                        
                            if (verbose) LogNodeCount(from, move, nextCount);
                        }
                    }
                }
            } else {
                Parallel.ForEach(colored, ParallelOptions, from =>
                {
                    MoveList moveList = new(board, from);
                    if (moveList.Count == 0) return;
                    
                    Board next = board.Clone();
                
                    BitBoard moves = moveList.Get();
                    
                    BitBoardMap originalState = next.GetCurrentState;
                    foreach (Square move in moves) {
                        next.Move(from, move);
                        ulong nextCount = MoveGeneration(next, depth - 1, Util.OppositeColor(color), false);
                        Interlocked.Add(ref count, nextCount);
                        next.UndoMove(ref originalState);
                        
                        if (verbose) LogNodeCount(from, move, nextCount);
                    }
                });
            }
          
            return count;
        }

    }

}