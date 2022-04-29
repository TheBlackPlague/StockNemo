using System;
using Backend;
using Backend.Board;
using Backend.Exception;
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
        
        private readonly DataBoard Board = new();

        private int selectedDepth = -1;
        
        public (int, int) Depth0()
        {
            selectedDepth = 0;
            return (D0, MoveGeneration(Board, 0));
        }
        
        public (int, int) Depth1()
        {
            selectedDepth = 1;
            return (D1, MoveGeneration(Board, 1));
        }
        
        public (int, int) Depth2()
        {
            selectedDepth = 2;
            return (D2, MoveGeneration(Board, 2));
        }

        public (int, int) Depth3()
        {
            selectedDepth = 3;
            return (D3, MoveGeneration(Board, 3));
        }

        public (int, int) Depth4()
        {
            selectedDepth = 4;
            return (D4, MoveGeneration(Board, 4));
        }
        
        public (int, int) Depth5()
        {
            selectedDepth = 5;
            return (D5, MoveGeneration(Board, 5));
        }
        
        public (int, int) Depth6()
        {
            selectedDepth = 6;
            return (D6, MoveGeneration(Board, 6));
        }
        
        private int MoveGeneration(DataBoard board, int depth, PieceColor color = PieceColor.White)
        {
            if (depth == 0) return 1;
            int count = 0;
            
            foreach ((int, int) piece in board.All(color)) {
                try {
                    LegalMoveSet moveSet = new(board, piece);
                    if (depth > 1) {
                        foreach ((int, int) move in moveSet.Get()) {
                            int previousCount = count;
                            DataBoard nextBoard = board.Clone();
                            nextBoard.SecureMove(piece, move);
                            count += MoveGeneration(nextBoard, depth - 1, Util.OppositeColor(color));

                            if (depth != selectedDepth) continue;
                            
                            string fullMove = Util.TupleToChessString(piece) + Util.TupleToChessString(move);
                            Console.WriteLine(fullMove.ToLower() + ": " + (count - previousCount));
                        }
                    } else count += moveSet.Count();
                } catch (InvalidMoveLookupException e) {
                    Console.WriteLine(e.Message + ", Color: " + color);
                    break;
                }
            }
            
            return count;
        }

    }

}