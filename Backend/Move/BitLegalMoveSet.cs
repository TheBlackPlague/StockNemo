using System;
using Backend.Board;

namespace Backend.Move
{

    public class BitLegalMoveSet
    {

        private const int ROOK_BITS = 12;
        private const int BISHOP_BITS = 9;

        private static readonly (BitBoard, int)[,] RookMagicData = {
            {
                (0x80280013FF84FFFF, 10890), (0x5FFBFEFDFEF67FFF, 50579), (0xFFEFFAFFEFFDFFFF, 62020),
                (0x003000900300008A, 67322), (0x0050028010500023, 80251), (0x0020012120A00020, 58503),
                (0x0030006000C00030, 51175), (0x0058005806B00002, 83130)
            },
            {
                (0x7FBFF7FBFBEAFFFC, 50430), (0x0000140081050002, 21613), (0x0000180043800048, 72625), 
                (0x7FFFE800021FFFB8, 80755), (0xFFFFCFFE7FCFFFAF, 69753), (0x00001800C0180060, 26973), 
                (0x4F8018005FD00018, 84972), (0x0000180030620018, 31958)
            },
            {
                (0x00300018010C0003, 69272), (0x0003000C0085FFFF, 48372), (0xFFFDFFF7FBFEFFF7, 65477), 
                (0x7FC1FFDFFC001FFF, 43972), (0xFFFEFFDFFDFFDFFF, 57154), (0x7C108007BEFFF81F, 53521), 
                (0x20408007BFE00810, 30534), (0x0400800558604100, 16548)
            },
            {
                (0x0040200010080008, 46407), (0x0010020008040004, 11841), (0xFFFDFEFFF7FBFFF7, 21112),
                (0xFEBF7DFFF8FEFFF9, 44214), (0xC00000FFE001FFE0, 57925), (0x4AF01F00078007C3, 29574),
                (0xBFFBFAFFFB683F7F, 17309), (0x0807F67FFA102040, 40143)
            },
            {
                (0x200008E800300030, 64659), (0x0000008780180018, 70469), (0x0000010300180018, 62917), 
                (0x4000008180180018, 60997), (0x008080310005FFFA, 18554), (0x4000188100060006, 14385), 
                (0xFFFFFF7FFFBFBFFF,     0), (0x0000802000200040, 38091)
            },
            {
                (0x20000202EC002800, 25122), (0xFFFFF9FF7CFFF3FF, 60083), (0x000000404B801800, 72209), 
                (0x2000002FE03FD000, 67875), (0xFFFFFF6FFE7FCFFD, 56290), (0xBFF7EFFFBFC00FFF, 43807), 
                (0x000000100800A804, 73365), (0x6054000A58005805, 76398)
            },
            {
                (0x0829000101150028, 20024), (0x00000085008A0014,  9513), (0x8000002B00408028, 24324),
                (0x4000002040790028, 22996), (0x7800002010288028, 23213), (0x0000001800E08018, 56002),
                (0xA3A80003F3A40048, 22809), (0x2003D80000500028, 44545)
            },
            {
                (0xFFFFF37EEFEFDFBE, 36072), (0x40000280090013C1,  4750), (0xBF7FFEFFBFFAF71F,  6014), 
                (0xFFFDFFFF777B7D6E, 36054), (0x48300007E8080C02, 78538), (0xAFE0000FFF780402, 28745), 
                (0xEE73FFFBFFBB77FE,  8555), (0x0002000308482882,  1009)
            }
        };
        private static readonly (BitBoard, int)[,] BishopMagicData = {
            {
                (0xA7020080601803D8, 60984), (0x13802040400801F1, 66046), (0x0A0080181001F60C, 32910),
                (0x1840802004238008, 16369), (0xC03FE00100000000, 42115), (0x24C00BFFFF400000, 835),
                (0x0808101F40007F04, 18910), (0x100808201EC00080, 25911)
            },

            {
                (0xFFA2FEFFBFEFB7FF, 63301), (0x083E3EE040080801, 16063), (0xC0800080181001F8, 17481),
                (0x0440007FE0031000, 59361), (0x2010007FFC000000, 18735), (0x1079FFE000FF8000, 61249),
                (0x3C0708101F400080, 68938), (0x080614080FA00040, 61791)
            },
            {
                (0x7FFE7FFF817FCFF9, 21893), (0x7FFEBFFFA01027FD, 62068), (0x53018080C00F4001, 19829),
                (0x407E0001000FFB8A, 26091), (0x201FE000FFF80010, 15815), (0xFFDFEFFFDE39FFEF, 16419),
                (0xCC8808000FBF8002, 59777), (0x7FF7FBFFF8203FFF, 16288)
            },
            {
                (0x8800013E8300C030, 33235), (0x0420009701806018, 15459), (0x7FFEFF7F7F01F7FD, 15863),
                (0x8700303010C0C006, 75555), (0xC800181810606000, 79445), (0x20002038001C8010, 15917),
                (0x087FF038000FC001, 8512), (0x00080C0C00083007, 73069)
            },
            {
                (0x00000080FC82C040, 16078), (0x000000407E416020, 19168), (0x00600203F8008020, 11056),
                (0xD003FEFE04404080, 62544), (0xA00020C018003088, 80477), (0x7FBFFE700BFFE800, 75049),
                (0x107FF00FE4000F90, 32947), (0x7F8FFFCFF1D007F8, 59172)
            },
            {
                (0x0000004100F88080, 55845), (0x00000020807C4040, 61806), (0x00000041018700C0, 73601),
                (0x0010000080FC4080, 15546), (0x1000003C80180030, 45243), (0xC10000DF80280050, 20333),
                (0xFFFFFFBFEFF80FDC, 33402), (0x000000101003F812, 25917)
            },
            {
                (0x0800001F40808200, 32875), (0x084000101F3FD208, 4639), (0x080000000F808081, 17077),
                (0x0004000008003F80, 62324), (0x08000001001FE040, 18159), (0x72DD000040900A00, 61436),
                (0xFFFFFEFFBFEFF81D, 57073), (0xCD8000200FEBF209, 61025)
            },
            {
                (0x100000101EC10082, 81259), (0x7FBAFFFFEFE0C02F, 64083), (0x7F83FFFFFFF07F7F, 56114),
                (0xFFF1FFFFFFF7FFC1, 57058), (0x0878040000FFE01F, 58912), (0x945E388000801012, 22194),
                (0x0840800080200FDA, 70880), (0x100000C05F582008, 11140)
            }
        };
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
        private static (BitBoard, BitBoard, int)[,] RookMagic =
            new (BitBoard, BitBoard, int)[BitDataBoard.UBOUND, BitDataBoard.UBOUND];
        private static (BitBoard, BitBoard, int)[,] BishopMagic =
            new (BitBoard, BitBoard, int)[BitDataBoard.UBOUND, BitDataBoard.UBOUND];
        private static BitBoard[] SlidingMoves = new BitBoard[87988];
        
        private readonly BitDataBoard Board;
        private readonly BitBoard From;
        private readonly int H;
        private readonly int V;
        
        private static BitBoard GetRookBlockers(int v, int h)
        {
            BitBoard hMoves = BitBoard.Hs[h] & ~(BitBoard.Vs[0] | BitBoard.Vs[7]);
            BitBoard vMoves = BitBoard.Vs[v] & ~(BitBoard.Hs[0] | BitBoard.Hs[7]);

            return (hMoves | vMoves) & ~(BitBoard)(h, v);
        }
        private static BitBoard GetBishopBlockers(int v, int h)
        {
            BitBoard rays = BitBoard.Default;
            int i = 0;
            while (i < 64) {
                int hI = i % 8;
                int vI = i / 8;

                int hD = Math.Abs((sbyte)(ulong)BitBoard.Hs[h] - (sbyte)(ulong)BitBoard.Hs[hI]);
                int vD = Math.Abs((sbyte)(ulong)BitBoard.Vs[v] - (sbyte)(ulong)BitBoard.Vs[vI]);

                if (hD == vD && vD != 0) rays |= 1UL << i;

                i++;
            }

            return rays & ~BitBoard.Edged;
        }

        private static void GenerateRookMasks()
        {
            for (int h = 0; h < BitDataBoard.UBOUND; h++)
            for (int v = 0; v < BitDataBoard.UBOUND; v++) {
                (BitBoard magic, int offset) = RookMagicData[v, h];
                RookMagic[v, h] = (magic, GetRookBlockers(v, h), offset);
                Console.WriteLine("Generated rook magic: " + (h, v));
            }
        }
        
        private static void GenerateBishopMasks()
        {
            for (int h = 0; h < BitDataBoard.UBOUND; h++)
            for (int v = 0; v < BitDataBoard.UBOUND; v++) {
                (BitBoard magic, int offset) = BishopMagicData[v, h];
                BishopMagic[v, h] = (magic, GetBishopBlockers(v, h), offset);
                Console.WriteLine("Generated bishop magic: " + (h, v));
            }
        }

        private static int GetMagicIndex(
            ref (BitBoard, BitBoard, int)[,] magics, int indexBits, BitBoard blockers, int h, int v
        )
        {
            (BitBoard magic, BitBoard mask, int offset) = magics[v, h];
            BitBoard relevantBlockers = blockers | mask;
            BitBoard hash = relevantBlockers * magic;
            return offset + (int)(ulong)(hash >> (64 - indexBits));
        }

        private static void GenerateSlidingMoves(
            ref BitBoard[] moveTable, ref (BitBoard, BitBoard, int)[,] magics, int indexBits, (int, int)[] deltas
        )
        {
            for (int h = 0; h < BitDataBoard.UBOUND; h++)
            for (int v = 0; v < BitDataBoard.UBOUND; v++) {
                (_, BitBoard mask, _) = magics[v, h];
                mask = ~mask;
                
                BitBoard blockers = BitBoard.Default;
                while (true) {
                    BitBoard moves = BitBoard.Default;
                    foreach ((int dH, int dV) in deltas) {
                        (int hI, int vI) = (h, v);
                        while (!blockers[hI, vI]) {
                            if (hI + dH is > 7 or < 0 || vI + dV is > 7 or < 0) break;
                            
                            hI += dH;
                            vI += dV;
                            moves |= (hI, vI);
                        }
                    }

                    moveTable[GetMagicIndex(ref magics, indexBits, blockers, h, v)] = moves;

                    blockers = (blockers - mask) & mask;
                    if (blockers.Count() == 0) break;
                }
                
                Console.WriteLine("Generated sliding moves for [" + (indexBits == ROOK_BITS ? "Rook" : "Bishop") + "]: "
                                  + (h, v));
            }
        }

        public static void SetUp()
        {
            GenerateRookMasks();
            GenerateBishopMasks();
            
            GenerateSlidingMoves(ref SlidingMoves, ref RookMagic, ROOK_BITS, new []
            {
                (1, 0),
                (0, -1),
                (-1, 0),
                (0, 1)
            });
            
            GenerateSlidingMoves(ref SlidingMoves, ref BishopMagic, BISHOP_BITS, new []
            {
                (1, 1),
                (1, -1),
                (-1, -1),
                (-1, 1)
            });
        }

        private BitBoard Moves = BitBoard.Default;
        public int Count => Moves.Count();

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
                    // LegalRookMoveSet(color);
                    break;
                case Piece.Knight:
                    LegalKnightMoveSet(color);
                    break;
                case Piece.Bishop:
                    // LegalBishopMoveSet(color);
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
            int mIndex = GetMagicIndex(ref RookMagic, ROOK_BITS, ~Board.All(PieceColor.None), H, V);
            Moves = SlidingMoves[mIndex];
            Moves &= ~Board.All(color);
        }

        private void LegalKnightMoveSet(PieceColor color)
        {
            Moves = KnightMoves[V, H];
            Moves &= ~Board.All(color);
        }
        
        private void LegalBishopMoveSet(PieceColor color)
        {
            int mIndex = GetMagicIndex(ref RookMagic, BISHOP_BITS, ~Board.All(PieceColor.None), H, V);
            Moves = SlidingMoves[mIndex];
            Moves &= ~Board.All(color);
        }


    }

}