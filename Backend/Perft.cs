using System;
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
        
        private readonly Board Board = Board.Default();

        private int SelectedDepth;

        private static void LogNodeCount((int, int) piece, (int, int) move, ulong nodeC)
        {
            string fullMove = Util.TupleToChessString(piece) + Util.TupleToChessString(move);
            Console.WriteLine(fullMove.ToLower() + ": " + nodeC);
        }
        
        private static void LogNodeCount((int, int) piece, int nodeC)
        {
            Console.WriteLine(Util.TupleToChessString(piece).ToLower() + ": " + nodeC);
        }

        public Perft()
        {
            // Draw the board being tested.
            Console.WriteLine(Board.ToString());
            
            // Involve JIT.
            SelectedDepth = 4;
            MoveGeneration(Board, 4, verbose: false);
        }
        
        public (ulong, ulong) Depth0()
        {
            SelectedDepth = 0;
            return (D0, MoveGeneration(Board.Clone(), 0));
        }
        
        public (ulong, ulong) Depth1()
        {
            SelectedDepth = 1;
            return (D1, MoveGeneration(Board.Clone(), 1));
        }
        
        public (ulong, ulong) Depth2()
        {
            SelectedDepth = 2;
            return (D2, MoveGeneration(Board.Clone(), 2));
        }

        public (ulong, ulong) Depth3()
        {
            SelectedDepth = 3;
            return (D3, MoveGeneration(Board.Clone(), 3));
        }

        public (ulong, ulong) Depth4()
        {
            SelectedDepth = 4;
            return (D4, MoveGeneration(Board.Clone(), 4));
        }
        
        public (ulong, ulong) Depth5()
        {
            SelectedDepth = 5;
            return (D5, MoveGeneration(Board.Clone(), 5));
        }
        
        public (ulong, ulong) Depth6()
        {
            SelectedDepth = 6;
            return (D6, MoveGeneration(Board.Clone(), 6));
        }
        
        public (ulong, ulong) Depth7()
        {
            SelectedDepth = 7;
            return (D7, MoveGeneration(Board, 7));
        }
        
        private ulong MoveGeneration(Board board, int depth, PieceColor color = PieceColor.White, bool verbose = true)
        {
            if (depth == 0) return 1;
            ulong count = 0;
            
            if (depth == SelectedDepth) color = board.WhiteTurn ? PieceColor.White : PieceColor.Black;
            PieceColor oppositeColor = Util.OppositeColor(color);
            int nextDepth = depth - 1;
         
            BitBoard colored = board.All(color);
            if (depth < 5) {
                foreach ((int, int) from in colored) {
                    MoveList moveList = new(board, from);
                    if (depth == 1) {
                        count += (ulong)moveList.Count;
                    
                        if (depth != SelectedDepth) continue;
                    
                        if (verbose) LogNodeCount(from, moveList.Count);
                    } else {
                        if (moveList.Count == 0) continue;

                        BitBoardMap originalState = board.GetCurrentState;
                        foreach ((int, int) move in moveList) {
                            board.Move(from, move);
                            ulong nextCount = MoveGeneration(board, nextDepth, oppositeColor);
                            count += nextCount;
                            board.UndoMove(ref originalState);
                        
                            if (depth != SelectedDepth) continue;
                        
                            if (verbose) LogNodeCount(from, move, nextCount);
                        }
                    }
                }
            } else {
                Parallel.ForEach(colored, from =>
                {
                    MoveList moveList = new(board, from);
                    if (moveList.Count == 0) return;
                    
                    Board next = board.Clone();
                
                    BitBoard moves = moveList.Get();
                    
                    BitBoardMap originalState = board.GetCurrentState;
                    foreach ((int, int) move in moves) {
                        next.Move(from, move);
                        ulong nextCount = MoveGeneration(next, depth - 1, Util.OppositeColor(color));
                        Interlocked.Add(ref count, nextCount);
                        next.UndoMove(ref originalState);
                        
                        if (depth != SelectedDepth) continue;
                        
                        if (verbose) LogNodeCount(from, move, nextCount);
                    }
                });
            }
          
            return count;
        }

    }

}