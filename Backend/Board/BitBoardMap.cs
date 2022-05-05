using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Backend.Board
{

    public class BitBoardMap
    {
        
        private const string FEN_SPR = "/";

        // White
        // ReSharper disable once InconsistentNaming
        private BitBoard WPB;
        // ReSharper disable once InconsistentNaming
        private BitBoard WRB;
        // ReSharper disable once InconsistentNaming
        private BitBoard WNB;
        // ReSharper disable once InconsistentNaming
        private BitBoard WBB;
        // ReSharper disable once InconsistentNaming
        private BitBoard WQB;
        // ReSharper disable once InconsistentNaming
        private BitBoard WKB;
        
        // Black
        // ReSharper disable once InconsistentNaming
        private BitBoard BPB;
        // ReSharper disable once InconsistentNaming
        private BitBoard BRB;
        // ReSharper disable once InconsistentNaming
        private BitBoard BNB;
        // ReSharper disable once InconsistentNaming
        private BitBoard BBB;
        // ReSharper disable once InconsistentNaming
        private BitBoard BQB;
        // ReSharper disable once InconsistentNaming
        private BitBoard BKB;
        
        private static void Move(ref BitBoard board, (int, int) from, (int, int) to)
        {
            if (!board[from.Item1, from.Item2]) 
                throw new InvalidDataException("No piece was found at: " + from);

            board[from.Item1, from.Item2] = false;
            board[to.Item1, to.Item2] = true;
        }

        private BitBoardMap(BitBoardMap map)
        {
            WPB = map.WPB.Clone();
            WRB = map.WRB.Clone();
            WNB = map.WNB.Clone();
            WBB = map.WBB.Clone();
            WQB = map.WQB.Clone();
            WKB = map.WKB.Clone();
            BPB = map.BPB.Clone();
            BRB = map.BRB.Clone();
            BNB = map.BNB.Clone();
            BBB = map.BBB.Clone();
            BQB = map.BQB.Clone();
            BKB = map.BKB.Clone();
        }

        public BitBoardMap(string boardFen)
        {
            WPB = new BitBoard(BitBoard.Default);
            WRB = new BitBoard(BitBoard.Default);
            WNB = new BitBoard(BitBoard.Default);
            WBB = new BitBoard(BitBoard.Default);
            WQB = new BitBoard(BitBoard.Default);
            WKB = new BitBoard(BitBoard.Default);
            BPB = new BitBoard(BitBoard.Default);
            BRB = new BitBoard(BitBoard.Default);
            BNB = new BitBoard(BitBoard.Default);
            BBB = new BitBoard(BitBoard.Default);
            BQB = new BitBoard(BitBoard.Default);
            BKB = new BitBoard(BitBoard.Default);
            
            string[] expandedBoardData = boardFen.Split(FEN_SPR).Reverse().ToArray();
            if (expandedBoardData.Length != BitDataBoard.UBOUND) 
                throw new InvalidDataException("Wrong board data provided: " + boardFen);

            for (int v = 0; v < BitDataBoard.UBOUND; v++) {
                string rankData = expandedBoardData[v];
                int h = 0;
                foreach (char p in rankData) {
                    if (char.IsNumber(p)) {
                        h += int.Parse(p.ToString());
                        continue;
                    }

                    if (char.IsUpper(p)) {
                        switch (p) {
                            case 'P':
                                WPB[h, v] = true;
                                break;
                            case 'R':
                                WRB[h, v] = true;
                                break;
                            case 'N':
                                WNB[h, v] = true;
                                break;
                            case 'B':
                                WBB[h, v] = true;
                                break;
                            case 'Q':
                                WQB[h, v] = true;
                                break;
                            case 'K':
                                WKB[h, v] = true;
                                break;
                        }
                    } else {
                        switch (p) {
                            case 'p':
                                BPB[h, v] = true;
                                break;
                            case 'r':
                                BRB[h, v] = true;
                                break;
                            case 'n':
                                BNB[h, v] = true;
                                break;
                            case 'b':
                                BBB[h, v] = true;
                                break;
                            case 'q':
                                BQB[h, v] = true;
                                break;
                            case 'k':
                                BKB[h, v] = true;
                                break;
                        }
                    }

                    h++;
                }
            }
        }

        public BitBoard Get(PieceColor color)
        {
            return color switch
            {
                PieceColor.White => WPB.Or(WRB).Or(WNB).Or(WBB).Or(WQB).Or(WKB),
                PieceColor.Black => BPB.Or(BRB).Or(BNB).Or(BBB).Or(BQB).Or(BKB),
                PieceColor.None => Get(PieceColor.White).Or(Get(PieceColor.Black)).Flip(),
                _ => throw new InvalidOperationException("Must provide a valid PieceColor.")
            };
        }

        public BitBoard Get(Piece piece, PieceColor color)
        {
            return color switch
            {
                PieceColor.White => piece switch
                {
                    Piece.Pawn => WPB,
                    Piece.Rook => WRB,
                    Piece.Knight => WNB,
                    Piece.Bishop => WBB,
                    Piece.Queen => WQB,
                    Piece.King => WKB,
                    Piece.Empty or _ => throw new InvalidOperationException("Must provide a piece type.")
                },
                PieceColor.Black => piece switch
                {
                    Piece.Pawn => BPB,
                    Piece.Rook => BRB,
                    Piece.Knight => BNB,
                    Piece.Bishop => BBB,
                    Piece.Queen => BQB,
                    Piece.King => BKB,
                    Piece.Empty or _ => throw new InvalidOperationException("Must provide a piece type.")
                },
                _ or PieceColor.None => throw new InvalidOperationException("Must provide a color.")
            };
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        public (Piece, PieceColor) this[int h, int v]
        {
            get
            {
                // White
                if (WPB[h, v]) return (Piece.Pawn, PieceColor.White);
                if (WRB[h, v]) return (Piece.Rook, PieceColor.White);
                if (WNB[h, v]) return (Piece.Knight, PieceColor.White);
                if (WBB[h, v]) return (Piece.Bishop, PieceColor.White);
                if (WQB[h, v]) return (Piece.Queen, PieceColor.White);
                if (WKB[h, v]) return (Piece.King, PieceColor.White);
            
                // Black
                if (BPB[h, v]) return (Piece.Pawn, PieceColor.Black);
                if (BRB[h, v]) return (Piece.Rook, PieceColor.Black);
                if (BNB[h, v]) return (Piece.Knight, PieceColor.Black);
                if (BBB[h, v]) return (Piece.Bishop, PieceColor.Black);
                if (BQB[h, v]) return (Piece.Queen, PieceColor.Black);
                if (BKB[h, v]) return (Piece.King, PieceColor.Black);

                return (Piece.Empty, PieceColor.None);
            }
        }

        public void Move((int, int) from, (int, int) to)
        {
            // White
            if (WPB[from.Item1, from.Item2]) Move(ref WPB, from, to);
            if (WRB[from.Item1, from.Item2]) Move(ref WRB, from, to);
            if (WNB[from.Item1, from.Item2]) Move(ref WNB, from, to);
            if (WBB[from.Item1, from.Item2]) Move(ref WBB, from, to);
            if (WQB[from.Item1, from.Item2]) Move(ref WQB, from, to);
            if (WKB[from.Item1, from.Item2]) Move(ref WKB, from, to);
            
            // Black
            if (BPB[from.Item1, from.Item2]) Move(ref BPB, from, to);
            if (BRB[from.Item1, from.Item2]) Move(ref BRB, from, to);
            if (BNB[from.Item1, from.Item2]) Move(ref BNB, from, to);
            if (BBB[from.Item1, from.Item2]) Move(ref BBB, from, to);
            if (BQB[from.Item1, from.Item2]) Move(ref BQB, from, to);
            if (BKB[from.Item1, from.Item2]) Move(ref BKB, from, to);
        }

        public void Empty(int h, int v)
        {
            // White
            if (WPB[h, v]) WPB[h, v] = false;
            if (WRB[h, v]) WRB[h, v] = false;
            if (WNB[h, v]) WNB[h, v] = false;
            if (WBB[h, v]) WBB[h, v] = false;
            if (WQB[h, v]) WQB[h, v] = false;
            if (WKB[h, v]) WKB[h, v] = false;
            
            // Black
            if (BPB[h, v]) BPB[h, v] = false;
            if (BRB[h, v]) BRB[h, v] = false;
            if (BNB[h, v]) BNB[h, v] = false;
            if (BBB[h, v]) BBB[h, v] = false;
            if (BQB[h, v]) BQB[h, v] = false;
            if (BKB[h, v]) BKB[h, v] = false;
        }

        public BitBoardMap Clone()
        {
            return new BitBoardMap(this);
        }

        public bool Equals(BitBoardMap map)
        {
            return WPB.Equals(map.WPB) && WRB.Equals(map.WRB) && WNB.Equals(map.WNB) && WBB.Equals(map.WBB) &&
                   WQB.Equals(map.WQB) && WKB.Equals(map.WKB) && BPB.Equals(map.BPB) && BRB.Equals(map.BRB) &&
                   BNB.Equals(map.BNB) && BBB.Equals(map.BBB) && BQB.Equals(map.BQB) && BKB.Equals(map.BKB);
        }

    }

}