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

        private const int D0 = 1;
        private const int D1 = 20;
        private const int D2 = 400;
        private const int D3 = 8902;
        private const int D4 = 197281;
        private const int D5 = 4865609;
        private const int D6 = 119060324;
        
        private readonly DataBoard Board = DataBoard.Default();

        private int SelectedDepth;

        private static void LogNodeCount((int, int) piece, (int, int) move, int nodeC)
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
        
        public (int, int) Depth0()
        {
            SelectedDepth = 0;
            return (D0, MoveGeneration(Board, 0));
        }
        
        public (int, int) Depth1()
        {
            SelectedDepth = 1;
            return (D1, MoveGeneration(Board, 1));
        }
        
        public (int, int) Depth2()
        {
            SelectedDepth = 2;
            return (D2, MoveGeneration(Board, 2));
        }

        public (int, int) Depth3()
        {
            SelectedDepth = 3;
            return (D3, MoveGeneration(Board, 3));
        }

        public (int, int) Depth4()
        {
            SelectedDepth = 4;
            return (D4, MoveGeneration(Board, 4));
        }
        
        public (int, int) Depth5()
        {
            SelectedDepth = 5;
            return (D5, MoveGeneration(Board, 5));
        }
        
        public (int, int) Depth6()
        {
            SelectedDepth = 6;
            return (D6, MoveGeneration(Board, 6));
        }
        
        private int MoveGeneration(DataBoard board, int depth, PieceColor color = PieceColor.White, bool verbose = true)
        {
            if (depth == 0) return 1;
            int count = 0;
            
            if (depth == SelectedDepth) color = board.IsWhiteTurn() ? PieceColor.White : PieceColor.Black;

            if (depth == 1) {
                foreach ((int, int) piece in board.All(color)) {
                    LegalMoveSet moveSet = new(board, piece);
                    count += moveSet.Count();
                    if (depth != SelectedDepth) continue;
                    
                    if (verbose) LogNodeCount(piece, moveSet.Count());
                }

                return count;
            }
            
            Parallel.ForEach(board.All(color), piece =>
            {
                LegalMoveSet moveSet = new(board, piece);

                foreach ((int, int) move in moveSet.Get()) {
                    DataBoard nextBoard = board.Clone();
                    nextBoard.Move(piece, move);
                    int generatedCount = MoveGeneration(nextBoard, depth - 1, Util.OppositeColor(color));
                    Interlocked.Add(
                        ref count,
                        generatedCount
                    );
                    // board.Move(move, piece, true);

                    if (depth != SelectedDepth) continue;

                    if (verbose) LogNodeCount(piece, move, generatedCount);
                }
            });
            
            return count;
        }

    }

}