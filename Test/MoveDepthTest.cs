using System;
using System.Threading;
using System.Threading.Tasks;
using Backend;
using Backend.Board;
using Backend.Move;

namespace Test
{

    public class MoveDepthTest
    {

        private const ulong D0 = 1;
        private const ulong D1 = 20;
        private const ulong D2 = 400;
        private const ulong D3 = 8902;
        private const ulong D4 = 197281;
        private const ulong D5 = 4865609;
        private const ulong D6 = 119060324;
        
        private readonly DataBoard Board = DataBoard.Default();

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

        public MoveDepthTest()
        {
            // Draw the board being tested.
            Console.WriteLine(Board.ToString());
            
            // Involve JIT.
            SelectedDepth = 0;
            MoveGeneration(Board, 0, verbose: false);
            SelectedDepth = 1;
            MoveGeneration(Board, 1, verbose: false);
        }
        
        public (ulong, ulong) Depth0()
        {
            SelectedDepth = 0;
            return (D0, MoveGeneration(Board, 0));
        }
        
        public (ulong, ulong) Depth1()
        {
            SelectedDepth = 1;
            return (D1, MoveGeneration(Board, 1));
        }
        
        public (ulong, ulong) Depth2()
        {
            SelectedDepth = 2;
            return (D2, MoveGeneration(Board, 2));
        }

        public (ulong, ulong) Depth3()
        {
            SelectedDepth = 3;
            return (D3, MoveGeneration(Board, 3));
        }

        public (ulong, ulong) Depth4()
        {
            SelectedDepth = 4;
            return (D4, MoveGeneration(Board, 4));
        }
        
        public (ulong, ulong) Depth5()
        {
            SelectedDepth = 5;
            return (D5, MoveGeneration(Board, 5));
        }
        
        public (ulong, ulong) Depth6()
        {
            SelectedDepth = 6;
            return (D6, MoveGeneration(Board, 6));
        }
        
        private ulong MoveGeneration(DataBoard board, int depth, PieceColor color = PieceColor.White, bool verbose = true)
        {
            if (depth == 0) return 1;
            ulong count = 0;
            
            if (depth == SelectedDepth) color = board.IsWhiteTurn() ? PieceColor.White : PieceColor.Black;
            PieceColor oppositeColor = Util.OppositeColor(color);
            int nextDepth = depth - 1;

            BitBoard colored = board.All(color);
            if (depth < 5) {
                foreach ((int, int) from in colored) {
                    LegalMoveSet moveSet = new(board, from);
                    if (depth == 1) {
                        count += (ulong)moveSet.Count;
                    
                        if (depth != SelectedDepth) continue;
                    
                        if (verbose) LogNodeCount(from, moveSet.Count);
                    } else {
                        BitBoard moves = moveSet.Get();
                        if (moves.Count == 0) continue;
            
                        foreach ((int, int) move in moves) {
                            DataBoard next = board.Clone();
                            next.Move(from, move);
                            ulong nextCount = MoveGeneration(next, nextDepth, oppositeColor);
                            count += nextCount;
                        
                            if (depth != SelectedDepth) continue;
                        
                            if (verbose) LogNodeCount(from, move, nextCount);
                        }
                    }
                }
            } else {
                Parallel.ForEach(colored, from =>
                {
                    LegalMoveSet moveSet = new(board, from);
                    if (moveSet.Count == 0) return;
                
                    BitBoard moves = moveSet.Get();
                    
                    foreach ((int, int) move in moves) {
                        DataBoard next = board.Clone();
                        next.Move(from, move);
                        ulong nextCount = MoveGeneration(next, depth - 1, Util.OppositeColor(color));
                        Interlocked.Add(ref count, nextCount);
                        
                        if (depth != SelectedDepth) continue;
                        
                        if (verbose) LogNodeCount(from, move, nextCount);
                    }
                });
            }
            
            return count;
        }

    }

}