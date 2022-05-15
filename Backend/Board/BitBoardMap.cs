using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Backend.Board
{

    public struct BitBoardMap
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

        private BitBoard White;
        private BitBoard Black;
        
        public bool WhiteTurn;
        
        public bool WhiteKCastle;
        public bool WhiteQCastle;
        public bool BlackKCastle;
        public bool BlackQCastle;
        
        public BitBoard EnPassantTarget;
        
        private static void Move(ref BitBoard board, (int, int) from, (int, int) to)
        {
            board[from.Item1, from.Item2] = false;
            board[to.Item1, to.Item2] = true;
        }
        
        private static void Move(ref BitBoard fromBoard, ref BitBoard toBoard, (int, int) from, (int, int) to)
        {
            fromBoard[from.Item1, from.Item2] = false;
            toBoard[to.Item1, to.Item2] = false;
            fromBoard[to.Item1, to.Item2] = true;
        }

        public BitBoardMap(string boardFen, string turnData, string castlingData, string enPassantTargetData)
        {
            WPB = BitBoard.Default;
            WRB = BitBoard.Default;
            WNB = BitBoard.Default;
            WBB = BitBoard.Default;
            WQB = BitBoard.Default;
            WKB = BitBoard.Default;
            BPB = BitBoard.Default;
            BRB = BitBoard.Default;
            BNB = BitBoard.Default;
            BBB = BitBoard.Default;
            BQB = BitBoard.Default;
            BKB = BitBoard.Default;
            
            string[] expandedBoardData = boardFen.Split(FEN_SPR).Reverse().ToArray();
            if (expandedBoardData.Length != DataBoard.UBOUND) 
                throw new InvalidDataException("Wrong board data provided: " + boardFen);

            for (int v = 0; v < DataBoard.UBOUND; v++) {
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

            WhiteTurn = turnData[0] == 'w';
            WhiteKCastle = castlingData.Contains("K");
            WhiteQCastle = castlingData.Contains("Q");
            BlackKCastle = castlingData.Contains("k");
            BlackQCastle = castlingData.Contains("q");
            EnPassantTarget = BitBoard.Default;
            
            if (enPassantTargetData.Length == 2) {
                (int h, int v) = Util.ChessStringToTuple(enPassantTargetData.ToUpper());
                EnPassantTarget[h, v] = true;
            }

            White = WPB | WRB | WNB | WBB | WQB | WKB;
            Black = BPB | BRB | BNB | BBB | BQB | BKB;
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        public (Piece, PieceColor) this[int h, int v]
        {
            get
            {
                if (this[PieceColor.White][h, v]) {
                    if (WPB[h, v]) return (Piece.Pawn, PieceColor.White);
                    if (WRB[h, v]) return (Piece.Rook, PieceColor.White);
                    if (WNB[h, v]) return (Piece.Knight, PieceColor.White);
                    if (WBB[h, v]) return (Piece.Bishop, PieceColor.White);
                    if (WQB[h, v]) return (Piece.Queen, PieceColor.White);
                    if (WKB[h, v]) return (Piece.King, PieceColor.White);
                } else if (this[PieceColor.Black][h, v]) {
                    if (BPB[h, v]) return (Piece.Pawn, PieceColor.Black);
                    if (BRB[h, v]) return (Piece.Rook, PieceColor.Black);
                    if (BNB[h, v]) return (Piece.Knight, PieceColor.Black);
                    if (BBB[h, v]) return (Piece.Bishop, PieceColor.Black);
                    if (BQB[h, v]) return (Piece.Queen, PieceColor.Black);
                    if (BKB[h, v]) return (Piece.King, PieceColor.Black);
                }

                return (Piece.Empty, PieceColor.None);
            }
        }

        public BitBoard this[PieceColor color]
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            get
            {
                return color switch
                {
                    PieceColor.White => White,
                    PieceColor.Black => Black,
                    PieceColor.None => ~(White | Black),
                    _ => throw new InvalidOperationException("Must provide a valid PieceColor.")
                };
            }
        }

        public BitBoard this[Piece piece, PieceColor color]
        {
            get
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
        }

        public void Move((int, int) from, (int, int) to)
        {
            ref BitBoard fromBoard = ref WPB;
            ref BitBoard toBoard = ref WPB;
            bool toBoardSet = false;
            PieceColor f = PieceColor.White;
            PieceColor t = PieceColor.White;
            
            if (White[from.Item1, from.Item2]) {
                // White
                if (WPB[from.Item1, from.Item2]) fromBoard = ref WPB;
                if (WRB[from.Item1, from.Item2]) fromBoard = ref WRB;
                if (WNB[from.Item1, from.Item2]) fromBoard = ref WNB;
                if (WBB[from.Item1, from.Item2]) fromBoard = ref WBB;
                if (WQB[from.Item1, from.Item2]) fromBoard = ref WQB;
                if (WKB[from.Item1, from.Item2]) fromBoard = ref WKB;
            } else if (Black[from.Item1, from.Item2]) {
                // Black
                f = PieceColor.Black;
                if (BPB[from.Item1, from.Item2]) fromBoard = ref BPB;
                if (BRB[from.Item1, from.Item2]) fromBoard = ref BRB;
                if (BNB[from.Item1, from.Item2]) fromBoard = ref BNB;
                if (BBB[from.Item1, from.Item2]) fromBoard = ref BBB;
                if (BQB[from.Item1, from.Item2]) fromBoard = ref BQB;
                if (BKB[from.Item1, from.Item2]) fromBoard = ref BKB;
            } else {
                Console.WriteLine("White:\n" + this[PieceColor.White] + "\n");
                Console.WriteLine("Black:\n" + this[PieceColor.Black] + "\n");
                throw new InvalidOperationException("Cannot move empty piece: " + from);
            }
            
            if (White[to.Item1, to.Item2]) {
                toBoardSet = true;
                // White
                if (WRB[to.Item1, to.Item2]) toBoard = ref WRB;
                if (WNB[to.Item1, to.Item2]) toBoard = ref WNB;
                if (WPB[to.Item1, to.Item2]) toBoard = ref WPB;
                if (WBB[to.Item1, to.Item2]) toBoard = ref WBB;
                if (WQB[to.Item1, to.Item2]) toBoard = ref WQB;
                if (WKB[to.Item1, to.Item2]) toBoard = ref WKB;
            } else if (Black[to.Item1, to.Item2]) {
                toBoardSet = true;
                // Black
                t = PieceColor.Black;
                if (BPB[to.Item1, to.Item2]) toBoard = ref BPB;
                if (BRB[to.Item1, to.Item2]) toBoard = ref BRB;
                if (BNB[to.Item1, to.Item2]) toBoard = ref BNB;
                if (BBB[to.Item1, to.Item2]) toBoard = ref BBB;
                if (BQB[to.Item1, to.Item2]) toBoard = ref BQB;
                if (BKB[to.Item1, to.Item2]) toBoard = ref BKB;
            }

            if (toBoardSet) {
                Move(ref fromBoard, ref toBoard, from, to);
                if (t == PieceColor.White) White[to.Item1, to.Item2] = false;
                else Black[to.Item1, to.Item2] = false;
            } else Move(ref fromBoard, from, to);

            if (f == PieceColor.White) {
                White[from.Item1, from.Item2] = false;
                White[to.Item1, to.Item2] = true;
            } else {
                Black[from.Item1, from.Item2] = false;
                Black[to.Item1, to.Item2] = true;
            }
        }

        public void Empty(int h, int v)
        {
            if (White[h, v]) {
                // White
                White[h, v] = false;
                if (WPB[h, v]) WPB[h, v] = false;
                if (WRB[h, v]) WRB[h, v] = false;
                if (WNB[h, v]) WNB[h, v] = false;
                if (WBB[h, v]) WBB[h, v] = false;
                if (WQB[h, v]) WQB[h, v] = false;
                if (WKB[h, v]) WKB[h, v] = false;
            } else if (Black[h, v]) {
                // Black
                Black[h, v] = false;
                if (BPB[h, v]) BPB[h, v] = false;
                if (BRB[h, v]) BRB[h, v] = false;
                if (BNB[h, v]) BNB[h, v] = false;
                if (BBB[h, v]) BBB[h, v] = false;
                if (BQB[h, v]) BQB[h, v] = false;
                if (BKB[h, v]) BKB[h, v] = false;
            } else throw new InvalidOperationException("Attempting to set already empty piece empty.");
        }

        internal string GenerateBoardFen()
        {
            string[] expandedBoardData = new string[DataBoard.UBOUND];
            for (int v = 0; v < DataBoard.UBOUND; v++) {
                string rankData = "";
                for (int h = 0; h < DataBoard.UBOUND; h++) {
                    (Piece piece, PieceColor color) = this[h, v];
                    if (piece == Piece.Empty) {
                        int c = 1;
                        for (int i = h + 1; i < DataBoard.UBOUND; i++) {
                            if (this[i, v].Item1 == Piece.Empty) c++;
                            else break;
                        }

                        rankData += c.ToString();
                        h += c - 1;
                        continue;
                    }

                    string input = piece.ToString()[0].ToString();
                    if (piece == Piece.Knight) input = "N";
                    if (color == PieceColor.White) rankData += input;
                    else rankData += input.ToLower();
                }

                expandedBoardData[v] = rankData;
            }

            return string.Join(FEN_SPR, expandedBoardData.Reverse());
        }

    }

}