using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data.Struct
{

    public struct BitBoardMap
    {
        
        private const string FEN_SPR = "/";

        // White
        private static readonly (Piece, PieceColor) Wp = (Piece.Pawn, PieceColor.White);
        private static readonly (Piece, PieceColor) Wr = (Piece.Rook, PieceColor.White);
        private static readonly (Piece, PieceColor) Wn = (Piece.Knight, PieceColor.White);
        private static readonly (Piece, PieceColor) Wb = (Piece.Bishop, PieceColor.White);
        private static readonly (Piece, PieceColor) Wq = (Piece.Queen, PieceColor.White);
        private static readonly (Piece, PieceColor) Wk = (Piece.King, PieceColor.White);
        
        // Black
        private static readonly (Piece, PieceColor) Bp = (Piece.Pawn, PieceColor.Black);
        private static readonly (Piece, PieceColor) Br = (Piece.Rook, PieceColor.Black);
        private static readonly (Piece, PieceColor) Bn = (Piece.Knight, PieceColor.Black);
        private static readonly (Piece, PieceColor) Bb = (Piece.Bishop, PieceColor.Black);
        private static readonly (Piece, PieceColor) Bq = (Piece.Queen, PieceColor.Black);
        private static readonly (Piece, PieceColor) Bk = (Piece.King, PieceColor.Black);
        
        // Empty
        private static readonly (Piece, PieceColor) E = (Piece.Empty, PieceColor.None);

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
        
        public Square EnPassantTarget;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Move(ref BitBoard board, Square from, Square to)
        {
            board[from] = false;
            board[to] = true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Move(ref BitBoard fromBoard, ref BitBoard toBoard, Square from, Square to)
        {
            fromBoard[from] = false;
            toBoard[to] = false;
            fromBoard[to] = true;
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
            if (expandedBoardData.Length != Board.UBOUND) 
                throw new InvalidDataException("Wrong board data provided: " + boardFen);

            for (int v = 0; v < Board.UBOUND; v++) {
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
                                WPB[v * 8 + h] = true;
                                break;
                            case 'R':
                                WRB[v * 8 + h] = true;
                                break;
                            case 'N':
                                WNB[v * 8 + h] = true;
                                break;
                            case 'B':
                                WBB[v * 8 + h] = true;
                                break;
                            case 'Q':
                                WQB[v * 8 + h] = true;
                                break;
                            case 'K':
                                WKB[v * 8 + h] = true;
                                break;
                        }
                    } else {
                        switch (p) {
                            case 'p':
                                BPB[v * 8 + h] = true;
                                break;
                            case 'r':
                                BRB[v * 8 + h] = true;
                                break;
                            case 'n':
                                BNB[v * 8 + h] = true;
                                break;
                            case 'b':
                                BBB[v * 8 + h] = true;
                                break;
                            case 'q':
                                BQB[v * 8 + h] = true;
                                break;
                            case 'k':
                                BKB[v * 8 + h] = true;
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
            EnPassantTarget = Square.Na;
            
            if (enPassantTargetData.Length == 2) {
                EnPassantTarget = System.Enum.Parse<Square>(enPassantTargetData, true);
            }

            White = WPB | WRB | WNB | WBB | WQB | WKB;
            Black = BPB | BRB | BNB | BBB | BQB | BKB;
        }

        public (Piece, PieceColor) this[Square sq]
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            get
            {
                if (White[sq]) {
                    if (WPB[sq]) return Wp;
                    if (WRB[sq]) return Wr;
                    if (WNB[sq]) return Wn;
                    if (WBB[sq]) return Wb;
                    if (WQB[sq]) return Wq;
                    if (WKB[sq]) return Wk;
                } else if (Black[sq]) {
                    if (BPB[sq]) return Bp;
                    if (BRB[sq]) return Br;
                    if (BNB[sq]) return Bn;
                    if (BBB[sq]) return Bb;
                    if (BQB[sq]) return Bq;
                    if (BKB[sq]) return Bk;
                }

                return E;
            }
        }

        public readonly BitBoard this[PieceColor color]
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

        public readonly BitBoard this[Piece piece, PieceColor color]
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Move(Square from, Square to)
        {
            ref BitBoard fromBoard = ref WPB;
            ref BitBoard toBoard = ref WPB;
            bool toBoardSet = false;
            PieceColor f = PieceColor.White;
            PieceColor t = PieceColor.White;
            
            if (White[from]) {
                // White
                if (WPB[from]) fromBoard = ref WPB;
                if (WRB[from]) fromBoard = ref WRB;
                if (WNB[from]) fromBoard = ref WNB;
                if (WBB[from]) fromBoard = ref WBB;
                if (WQB[from]) fromBoard = ref WQB;
                if (WKB[from]) fromBoard = ref WKB;
            } else if (Black[from]) {
                // Black
                f = PieceColor.Black;
                if (BPB[from]) fromBoard = ref BPB;
                if (BRB[from]) fromBoard = ref BRB;
                if (BNB[from]) fromBoard = ref BNB;
                if (BBB[from]) fromBoard = ref BBB;
                if (BQB[from]) fromBoard = ref BQB;
                if (BKB[from]) fromBoard = ref BKB;
            } else {
                Console.WriteLine("White:\n" + this[PieceColor.White] + "\n");
                Console.WriteLine("Black:\n" + this[PieceColor.Black] + "\n");
                throw new InvalidOperationException("Cannot move empty piece: " + from);
            }
            
            if (White[to]) {
                toBoardSet = true;
                // White
                if (WRB[to]) toBoard = ref WRB;
                if (WNB[to]) toBoard = ref WNB;
                if (WPB[to]) toBoard = ref WPB;
                if (WBB[to]) toBoard = ref WBB;
                if (WQB[to]) toBoard = ref WQB;
                if (WKB[to]) toBoard = ref WKB;
            } else if (Black[to]) {
                toBoardSet = true;
                // Black
                t = PieceColor.Black;
                if (BPB[to]) toBoard = ref BPB;
                if (BRB[to]) toBoard = ref BRB;
                if (BNB[to]) toBoard = ref BNB;
                if (BBB[to]) toBoard = ref BBB;
                if (BQB[to]) toBoard = ref BQB;
                if (BKB[to]) toBoard = ref BKB;
            }

            if (toBoardSet) {
                Move(ref fromBoard, ref toBoard, from, to);
                if (t == PieceColor.White) White[to] = false;
                else Black[to] = false;
            } else Move(ref fromBoard, from, to);

            if (f == PieceColor.White) {
                White[from] = false;
                White[to] = true;
            } else {
                Black[from] = false;
                Black[to] = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Empty(Square sq)
        {
            if (White[sq]) {
                // White
                White[sq] = false;
                if (WPB[sq]) WPB[sq] = false;
                if (WRB[sq]) WRB[sq] = false;
                if (WNB[sq]) WNB[sq] = false;
                if (WBB[sq]) WBB[sq] = false;
                if (WQB[sq]) WQB[sq] = false;
                if (WKB[sq]) WKB[sq] = false;
            } else if (Black[sq]) {
                // Black
                Black[sq] = false;
                if (BPB[sq]) BPB[sq] = false;
                if (BRB[sq]) BRB[sq] = false;
                if (BNB[sq]) BNB[sq] = false;
                if (BBB[sq]) BBB[sq] = false;
                if (BQB[sq]) BQB[sq] = false;
                if (BKB[sq]) BKB[sq] = false;
            } else throw new InvalidOperationException("Attempting to set already empty piece empty.");
        }

        internal string GenerateBoardFen()
        {
            string[] expandedBoardData = new string[Board.UBOUND];
            for (int v = 0; v < Board.UBOUND; v++) {
                string rankData = "";
                for (int h = 0; h < Board.UBOUND; h++) {
                    (Piece piece, PieceColor color) = this[(Square)(v * 8 + h)];
                    if (piece == Piece.Empty) {
                        int c = 1;
                        for (int i = h + 1; i < Board.UBOUND; i++) {
                            if (this[(Square)(v * 8 + i)].Item1 == Piece.Empty) c++;
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