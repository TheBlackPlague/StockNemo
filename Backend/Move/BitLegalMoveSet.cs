using System;
using System.IO;
using Backend.Board;

namespace Backend.Move
{

    public class BitLegalMoveSet
    {
        
        private static readonly BitBoard[,] WhitePawnAttacks = {
            { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 },
            { 0x20000, 0x50000, 0xa0000, 0x140000, 0x280000, 0x500000, 0xa00000, 0x400000 },
            { 0x2000000, 0x5000000, 0xa000000, 0x14000000, 0x28000000, 0x50000000, 0xa0000000, 0x40000000 },
            {
                0x200000000, 0x500000000, 0xa00000000, 0x1400000000, 0x2800000000, 0x5000000000, 0xa000000000,
                0x4000000000
            },
            {
                0x20000000000, 0x50000000000, 0xa0000000000, 0x140000000000, 0x280000000000, 0x500000000000,
                0xa00000000000, 0x400000000000
            },
            {
                0x2000000000000, 0x5000000000000, 0xa000000000000, 0x14000000000000, 0x28000000000000,
                0x50000000000000, 0xa0000000000000, 0x40000000000000
            },
            {
                0x200000000000000, 0x500000000000000, 0xa00000000000000, 0x1400000000000000, 0x2800000000000000,
                0x5000000000000000, 0xa000000000000000, 0x4000000000000000
            },
            { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 }
        };
        private static readonly BitBoard[,] BlackPawnAttacks = {
            { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 },
            { 0x2, 0x5, 0xa, 0x14, 0x28, 0x50, 0xa0, 0x40 },
            { 0x200, 0x500, 0xa00, 0x1400, 0x2800, 0x5000, 0xa000, 0x4000 },
            { 0x20000, 0x50000, 0xa0000, 0x140000, 0x280000, 0x500000, 0xa00000, 0x400000 },
            { 0x2000000, 0x5000000, 0xa000000, 0x14000000, 0x28000000, 0x50000000, 0xa0000000, 0x40000000 },
            { 
                0x200000000, 0x500000000, 0xa00000000, 0x1400000000, 0x2800000000, 0x5000000000, 0xa000000000, 
                0x4000000000
            },
            {
                0x20000000000, 0x50000000000, 0xa0000000000, 0x140000000000, 0x280000000000, 0x500000000000, 
                0xa00000000000, 0x400000000000
            },
            { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 }
        };
        private static readonly BitBoard[,] KnightMoves = {
            {
                0x0000000000020400, 0x0000000000050800, 0x00000000000A1100, 0x0000000000142200,
                0x0000000000284400, 0x0000000000508800, 0x0000000000A01000, 0x0000000000402000
            },
            {
                0x0000000002040004, 0x0000000005080008, 0x000000000A110011, 0x0000000014220022,
                0x0000000028440044, 0x0000000050880088, 0x00000000A0100010, 0x0000000040200020
            },
            {
                0x0000000204000402, 0x0000000508000805, 0x0000000A1100110A, 0x0000001422002214,
                0x0000002844004428, 0x0000005088008850, 0x000000A0100010A0, 0x0000004020002040
            },
            {
                0x0000020400040200, 0x0000050800080500, 0x00000A1100110A00, 0x0000142200221400,
                0x0000284400442800, 0x0000508800885000, 0x0000A0100010A000, 0x0000402000204000
            },
            {
                0x0002040004020000, 0x0005080008050000, 0x000A1100110A0000, 0x0014220022140000,
                0x0028440044280000, 0x0050880088500000, 0x00A0100010A00000, 0x0040200020400000
            },
            {
                0x0204000402000000, 0x0508000805000000, 0x0A1100110A000000, 0x1422002214000000,
                0x2844004428000000, 0x5088008850000000, 0xA0100010A0000000, 0x4020002040000000
            },
            {
                0x0400040200000000, 0x0800080500000000, 0x1100110A00000000, 0x2200221400000000,
                0x4400442800000000, 0x8800885000000000, 0x100010A000000000, 0x2000204000000000
            },
            {
                0x0004020000000000, 0x0008050000000000, 0x00110A0000000000, 0x0022140000000000,
                0x0044280000000000, 0x0088500000000000, 0x0010A00000000000, 0x0020400000000000
            }
        };
        
        private static readonly BitBoard[] SlidingMoves = new BitBoard[87988];
        
        private readonly BitDataBoard Board;
        private readonly BitBoard From;
        private readonly int H;
        private readonly int V;
        
        public int Count => Moves.Count;
        private BitBoard Moves = BitBoard.Default;

        private static void GenerateSlidingMoves(Piece piece)
        {
            ((BitBoard, BitBoard, int)[,], int) args = piece switch
            {
                Piece.Rook => (MagicBitBoard.RookMagic, MagicBitBoard.ROOK),
                Piece.Bishop => (MagicBitBoard.BishopMagic, MagicBitBoard.BISHOP),
                Piece.Pawn or Piece.Knight or Piece.Queen or Piece.King or Piece.Empty or _ => 
                    throw new InvalidDataException("No magic table found.")
            };
            (int, int)[] deltas = piece switch
            {
                Piece.Rook => new[]
                {
                    (1, 0),
                    (0, -1),
                    (-1, 0),
                    (0, 1)
                },
                Piece.Bishop => new[]
                {
                    (1, 1),
                    (1, -1),
                    (-1, -1),
                    (-1, 1)
                },
                Piece.Pawn or Piece.Knight or Piece.Queen or Piece.King or Piece.Empty or _ =>
                    throw new InvalidDataException("No magic table found.")
            };
            
            for (int h = 0; h < BitDataBoard.UBOUND; h++)
            for (int v = 0; v < BitDataBoard.UBOUND; v++) {
                (_, BitBoard mask, _) = args.Item1[v, h];
                mask = ~mask;
                
                BitBoard occupied = BitBoard.Default;
                while (true) {
                    BitBoard moves = BitBoard.Default;
                    foreach ((int dH, int dV) in deltas) {
                        int hI = h;
                        int vI = v;
                        
                        while (!occupied[hI, vI]) {
                            if (hI + dH is > 7 or < 0 || vI + dV is > 7 or < 0) break;
                            
                            hI += dH;
                            vI += dV;
                            moves |= (hI, vI);
                        }
                    }

                    SlidingMoves[MagicBitBoard.GetMagicIndex(piece, occupied, h, v)] = moves;

                    occupied = (occupied - mask) & mask;
                    if (occupied.Count == 0) break;
                }
                
                string chsStr = Util.TupleToChessString((h, v));
                Console.WriteLine("Generated Black Magic Attack Table for [" + piece + "] at: " + chsStr);
            }
        }

        static BitLegalMoveSet()
        {
            GenerateSlidingMoves(Piece.Rook);
            GenerateSlidingMoves(Piece.Bishop);
        }

        public BitLegalMoveSet(BitDataBoard board, (int, int) from, bool verify = true)
        {
            Board = board;
            From = from;

            (H, V) = from;
            
            (Piece piece, PieceColor color) = Board.At(from);
            // Generate Pseudo-Legal Moves
            switch (piece) {
                case Piece.Pawn:
                    LegalPawnMoveSet(color, !verify);
                    break;
                case Piece.Rook:
                    LegalRookMoveSet(color);
                    break;
                case Piece.Knight:
                    LegalKnightMoveSet(color);
                    break;
                case Piece.Bishop:
                    LegalBishopMoveSet(color);
                    break;
                case Piece.Queen:
                    // LegalQueenMoveSet(color);
                    break;
                case Piece.King:
                    // LegalKingMoveSet(color);
                    break;
                case Piece.Empty:
                default:
                    // throw InvalidMoveLookupException.FromBoard(
                    //     board, 
                    //     "Cannot generate move for empty piece: " + Util.TupleToChessString(from)
                    // );
                    break;
            }
        }

        public BitBoard Get()
        {
            return Moves;
        }

        private void LegalPawnMoveSet(PieceColor color, bool checkMovesOnly = false)
        {
            if (!checkMovesOnly) {
                // Normal
                // 1 Push
                Moves |= color == PieceColor.White ? From << 8 : From >> 8;
                
                if (V is 1 or 6 && Moves) {
                    // 2 Push
                    Moves |= color == PieceColor.White ? From << 16 : From >> 16;
                }

                BitBoard enPassantTarget = Board.GetEnPassantTarget();
                if (enPassantTarget) {
                    BitBoard targetExist = (color == PieceColor.White ? enPassantTarget << 8 : enPassantTarget >> 8) &
                                           Board.All(Util.OppositeColor(color));
                    if (targetExist) Moves |= enPassantTarget;
                }
            }
            
            // Attack Moves
            BitBoard attack = color == PieceColor.White ? WhitePawnAttacks[V, H] : BlackPawnAttacks[V, H];

            Moves |= attack;
            Moves &= ~Board.All(color);
        }

        private void LegalRookMoveSet(PieceColor color)
        {
            int mIndex = MagicBitBoard.GetMagicIndex(Piece.Rook, ~Board.All(PieceColor.None), H, V);
            Moves |= SlidingMoves[mIndex];
            Moves &= ~Board.All(color);
        }

        private void LegalKnightMoveSet(PieceColor color)
        {
            Moves |= KnightMoves[V, H];
            Moves &= ~Board.All(color);
        }
        
        private void LegalBishopMoveSet(PieceColor color)
        {
            int mIndex = MagicBitBoard.GetMagicIndex(Piece.Bishop, ~Board.All(PieceColor.None), H, V);
            Moves |= SlidingMoves[mIndex];
            Moves &= ~Board.All(color);
        }

    }

}