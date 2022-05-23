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

        // Empty
        private static readonly (Piece, PieceColor) E = (Piece.Empty, PieceColor.None);

        private readonly BitBoard[][] Bb;
        private readonly byte[] PiecesAndColors;

        private BitBoard White;
        private BitBoard Black;
        
        public bool WhiteTurn;
        
        public bool WhiteKCastle;
        public bool WhiteQCastle;
        public bool BlackKCastle;
        public bool BlackQCastle;
        
        public Square EnPassantTarget;

        public BitBoardMap(string boardFen, string turnData, string castlingData, string enPassantTargetData)
        {
            PiecesAndColors = new byte[64];
            for (int i = 0; i < 64; i++) PiecesAndColors[i] = 0x20;

            Bb = new[] {
                new [] {
                    BitBoard.Default, BitBoard.Default, BitBoard.Default, 
                    BitBoard.Default, BitBoard.Default, BitBoard.Default
                },
                new [] {
                    BitBoard.Default, BitBoard.Default, BitBoard.Default, 
                    BitBoard.Default, BitBoard.Default, BitBoard.Default
                }
            };

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
                                Bb[(int)PieceColor.White][(int)Piece.Pawn][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x0;
                                break;
                            case 'R':
                                Bb[(int)PieceColor.White][(int)Piece.Rook][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x1;
                                break;
                            case 'N':
                                Bb[(int)PieceColor.White][(int)Piece.Knight][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x2;
                                break;
                            case 'B':
                                Bb[(int)PieceColor.White][(int)Piece.Bishop][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x3;
                                break;
                            case 'Q':
                                Bb[(int)PieceColor.White][(int)Piece.Queen][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x4;
                                break;
                            case 'K':
                                Bb[(int)PieceColor.White][(int)Piece.King][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x5;
                                break;
                        }
                    } else {
                        switch (p) {
                            case 'p':
                                Bb[(int)PieceColor.Black][(int)Piece.Pawn][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x10;
                                break;
                            case 'r':
                                Bb[(int)PieceColor.Black][(int)Piece.Rook][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x11;
                                break;
                            case 'n':
                                Bb[(int)PieceColor.Black][(int)Piece.Knight][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x12;
                                break;
                            case 'b':
                                Bb[(int)PieceColor.Black][(int)Piece.Bishop][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x13;
                                break;
                            case 'q':
                                Bb[(int)PieceColor.Black][(int)Piece.Queen][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x14;
                                break;
                            case 'k':
                                Bb[(int)PieceColor.Black][(int)Piece.King][v * 8 + h] = true;
                                PiecesAndColors[v * 8 + h] = 0x15;
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

            White = Bb[(int)PieceColor.White][(int)Piece.Pawn] | Bb[(int)PieceColor.White][(int)Piece.Rook] | 
                    Bb[(int)PieceColor.White][(int)Piece.Knight] | Bb[(int)PieceColor.White][(int)Piece.Bishop] | 
                    Bb[(int)PieceColor.White][(int)Piece.Queen] | Bb[(int)PieceColor.White][(int)Piece.King];
            Black = Bb[(int)PieceColor.Black][(int)Piece.Pawn] | Bb[(int)PieceColor.Black][(int)Piece.Rook] | 
                    Bb[(int)PieceColor.Black][(int)Piece.Knight] | Bb[(int)PieceColor.Black][(int)Piece.Bishop] | 
                    Bb[(int)PieceColor.Black][(int)Piece.Queen] | Bb[(int)PieceColor.Black][(int)Piece.King];
        }

        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        private BitBoardMap(BitBoardMap map, BitBoard[][] bb, byte[] piecesAndColors)
        {
            White = map.White;
            Black = map.Black;
            WhiteKCastle = map.WhiteKCastle;
            WhiteQCastle = map.WhiteQCastle;
            BlackKCastle = map.BlackKCastle;
            BlackQCastle = map.BlackQCastle;
            WhiteTurn = map.WhiteTurn;
            EnPassantTarget = map.EnPassantTarget;
            
            PiecesAndColors = new byte[64];
            Bb = new BitBoard[2][];

            for (int i = 0; i < 2; i++) {
                Bb[i] = new BitBoard[6];
                Array.Copy(bb[i], Bb[i], 6);
            }
            Array.Copy(piecesAndColors, PiecesAndColors, 64);
        }

        public (Piece, PieceColor) this[Square sq]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                byte r = PiecesAndColors[(int)sq];
                return r switch
                {
                    < 0x6 => ((Piece)r, PieceColor.White),
                    < 0x16 => ((Piece)(r - 0x10), PieceColor.Black),
                    _ => E
                };
            }
        }

        public readonly BitBoard this[PieceColor color]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Bb[(int)color][(int)piece];
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Move(Square from, Square to)
        {
            (Piece pF, PieceColor cF) = this[from];
            (Piece pT, PieceColor cT) = this[to];

            if (pT != Piece.Empty) {
                Bb[(int)cT][(int)pT][to] = false;
                
                if (cT == PieceColor.White) White[to] = false;
                else Black[to] = false;
            }
            
            Bb[(int)cF][(int)pF][from] = false;
            Bb[(int)cF][(int)pF][to] = true;

            PiecesAndColors[(int)to] = PiecesAndColors[(int)from];
            PiecesAndColors[(int)from] = 0x20;

            if (cF == PieceColor.White) {
                White[from] = false;
                White[to] = true;
            } else {
                Black[from] = false;
                Black[to] = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Empty(Square sq)
        {
            (Piece p, PieceColor c) = this[sq];
            Bb[(int)c][(int)p][sq] = false;
            PiecesAndColors[(int)sq] = 0x20;

            if (c == PieceColor.White) White[sq] = false;
            else Black[sq] = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertPiece(Square sq, Piece piece, PieceColor color)
        {
            Bb[(int)color][(int)piece][sq] = true;
            if (color == PieceColor.White) White[sq] = true;
            else Black[sq] = true;
            
            int offset = color == PieceColor.White ? 0x0 : 0x10;
            PiecesAndColors[(int)sq] = (byte)((int)piece + offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoardMap Copy() => new(this, Bb, PiecesAndColors);

        public string GenerateBoardFen()
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