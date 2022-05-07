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
        
        private readonly BitDataBoard Board = BitDataBoard.Default();

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
        
        private ulong MoveGeneration(BitDataBoard board, int depth, PieceColor color = PieceColor.White, bool verbose = true)
        {
            if (depth == 0) return 1;
            ulong count = 0;
            
            if (depth == SelectedDepth) color = board.IsWhiteTurn() ? PieceColor.White : PieceColor.Black;

            BitBoard colored = ~board.All(color);
            for (int h = 0; h < BitDataBoard.UBOUND; h++)
            for (int v = 0; v < BitDataBoard.UBOUND; v++) {
                if (colored[h, v]) continue;
            
                BitLegalMoveSet moveSet = new(board, (h, v));
                if (depth == 1) {
                    count += (ulong)moveSet.Count;
                    
                    if (depth != SelectedDepth) continue;
                
                    if (verbose) LogNodeCount((h, v), moveSet.Count);
                } else {
                    BitBoard moves = ~moveSet.Get();
                    Console.WriteLine(moves.ToString());
                
                    for (int mH = 0; mH < BitDataBoard.UBOUND; mH++)
                    for (int mV = 0; mV < BitDataBoard.UBOUND; mV++) {
                        if (moves[mH, mV]) continue;
                
                        BitDataBoard next = Board.Clone();
                        next.Move((h, v), (mH, mV));
                        ulong nextCount = MoveGeneration(next, depth - 1, Util.OppositeColor(color));
                        count += nextCount;
                    
                        if (depth != SelectedDepth) continue;
                    
                        if (verbose) LogNodeCount((h, v), (mH, mV), nextCount);
                    }
                }
            }
            
            return count;
            
            // if (depth == 1) {
            //     for (int h = 0; h < BitDataBoard.UBOUND; h++)
            //     for (int v = 0; v < BitDataBoard.UBOUND; v++) {
            //         if (colored[h, v]) continue;
            //
            //         BitLegalMoveSet moveSet = new(board, (h, v));
            //         count += (ulong)moveSet.Count;
            //         if (depth != SelectedDepth) continue;
            //         
            //         if (verbose) LogNodeCount((h, v), moveSet.Count);
            //     }
            //
            //     return count;
            // }
            //
            // Parallel.For(0, BitDataBoard.UBOUND, h =>
            // {
            //     Parallel.For(0, BitDataBoard.UBOUND, v =>
            //     {
            //         if (colored[h, v]) return;
            //         
            //         BitLegalMoveSet moveSet = new(board, (h, v));
            //         BitBoard moves = ~moveSet.Get();
            //
            //         ulong interlockedCount = 0;
            //         for (int mH = 0; mH < BitDataBoard.UBOUND; mH++)
            //         for (int mV = 0; mV < BitDataBoard.UBOUND; mV++) {
            //             if (moves[mH, mV]) continue;
            //
            //             BitDataBoard next = Board.Clone();
            //             next.Move((h, v), (mH, mV));
            //             ulong nextCount = MoveGeneration(next, depth - 1, Util.OppositeColor(color));
            //             interlockedCount += count;
            //             
            //             if (depth != SelectedDepth) continue;
            //             
            //             if (verbose) LogNodeCount((h, v), (mH, mV), nextCount);
            //         }
            //
            //         Interlocked.Add(ref count, interlockedCount);
            //     });
            // });
        }

    }

}