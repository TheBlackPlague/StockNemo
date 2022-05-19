using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Data.Move;
using Backend.Exception;

namespace Backend.Data.Struct
{

    public struct MoveList : IEnumerable<Square>
    {

        #region Attack Tables
        
        private static readonly BitBoard[] WhitePawnAttacks = {
            0x0000000000000200, 0x0000000000000500, 0x0000000000000a00, 0x0000000000001400, 
            0x0000000000002800, 0x0000000000005000, 0x000000000000a000, 0x0000000000004000, 
            0x0000000000020000, 0x0000000000050000, 0x00000000000a0000, 0x0000000000140000, 
            0x0000000000280000, 0x0000000000500000, 0x0000000000a00000, 0x0000000000400000, 
            0x0000000002000000, 0x0000000005000000, 0x000000000a000000, 0x0000000014000000, 
            0x0000000028000000, 0x0000000050000000, 0x00000000a0000000, 0x0000000040000000, 
            0x0000000200000000, 0x0000000500000000, 0x0000000a00000000, 0x0000001400000000, 
            0x0000002800000000, 0x0000005000000000, 0x000000a000000000, 0x0000004000000000,
            0x0000020000000000, 0x0000050000000000, 0x00000a0000000000, 0x0000140000000000, 
            0x0000280000000000, 0x0000500000000000, 0x0000a00000000000, 0x0000400000000000, 
            0x0002000000000000, 0x0005000000000000, 0x000a000000000000, 0x0014000000000000, 
            0x0028000000000000, 0x0050000000000000, 0x00a0000000000000, 0x0040000000000000, 
            0x0200000000000000, 0x0500000000000000, 0x0a00000000000000, 0x1400000000000000, 
            0x2800000000000000, 0x5000000000000000, 0xa000000000000000, 0x4000000000000000, 
            0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 
            0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000
        };
        private static readonly BitBoard[] BlackPawnAttacks = {
            0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 
            0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
            0x0000000000000002, 0x0000000000000005, 0x000000000000000a, 0x0000000000000014, 
            0x0000000000000028, 0x0000000000000050, 0x00000000000000a0, 0x0000000000000040, 
            0x0000000000000200, 0x0000000000000500, 0x0000000000000a00, 0x0000000000001400, 
            0x0000000000002800, 0x0000000000005000, 0x000000000000a000, 0x0000000000004000, 
            0x0000000000020000, 0x0000000000050000, 0x00000000000a0000, 0x0000000000140000, 
            0x0000000000280000, 0x0000000000500000, 0x0000000000a00000, 0x0000000000400000, 
            0x0000000002000000, 0x0000000005000000, 0x000000000a000000, 0x0000000014000000, 
            0x0000000028000000, 0x0000000050000000, 0x00000000a0000000, 0x0000000040000000, 
            0x0000000200000000, 0x0000000500000000, 0x0000000a00000000, 0x0000001400000000, 
            0x0000002800000000, 0x0000005000000000, 0x000000a000000000, 0x0000004000000000, 
            0x0000020000000000, 0x0000050000000000, 0x00000a0000000000, 0x0000140000000000, 
            0x0000280000000000, 0x0000500000000000, 0x0000a00000000000, 0x0000400000000000, 
            0x0002000000000000, 0x0005000000000000, 0x000a000000000000, 0x0014000000000000, 
            0x0028000000000000, 0x0050000000000000, 0x00a0000000000000, 0x0040000000000000
        };
        private static readonly BitBoard[] KnightMoves = {
            0x0000000000020400, 0x0000000000050800, 0x00000000000A1100, 0x0000000000142200,
            0x0000000000284400, 0x0000000000508800, 0x0000000000A01000, 0x0000000000402000,
            0x0000000002040004, 0x0000000005080008, 0x000000000A110011, 0x0000000014220022,
            0x0000000028440044, 0x0000000050880088, 0x00000000A0100010, 0x0000000040200020,
            0x0000000204000402, 0x0000000508000805, 0x0000000A1100110A, 0x0000001422002214,
            0x0000002844004428, 0x0000005088008850, 0x000000A0100010A0, 0x0000004020002040,
            0x0000020400040200, 0x0000050800080500, 0x00000A1100110A00, 0x0000142200221400,
            0x0000284400442800, 0x0000508800885000, 0x0000A0100010A000, 0x0000402000204000,
            0x0002040004020000, 0x0005080008050000, 0x000A1100110A0000, 0x0014220022140000,
            0x0028440044280000, 0x0050880088500000, 0x00A0100010A00000, 0x0040200020400000,
            0x0204000402000000, 0x0508000805000000, 0x0A1100110A000000, 0x1422002214000000,
            0x2844004428000000, 0x5088008850000000, 0xA0100010A0000000, 0x4020002040000000,
            0x0400040200000000, 0x0800080500000000, 0x1100110A00000000, 0x2200221400000000,
            0x4400442800000000, 0x8800885000000000, 0x100010A000000000, 0x2000204000000000,
            0x0004020000000000, 0x0008050000000000, 0x00110A0000000000, 0x0022140000000000,
            0x0044280000000000, 0x0088500000000000, 0x0010A00000000000, 0x0020400000000000
        };
        private static readonly BitBoard[] KingMoves = {
            0x0000000000000302, 0x0000000000000705, 0x0000000000000E0A, 0x0000000000001C14,
            0x0000000000003828, 0x0000000000007050, 0x000000000000E0A0, 0x000000000000C040,
            0x0000000000030203, 0x0000000000070507, 0x00000000000E0A0E, 0x00000000001C141C,
            0x0000000000382838, 0x0000000000705070, 0x0000000000E0A0E0, 0x0000000000C040C0,
            0x0000000003020300, 0x0000000007050700, 0x000000000E0A0E00, 0x000000001C141C00,
            0x0000000038283800, 0x0000000070507000, 0x00000000E0A0E000, 0x00000000C040C000,
            0x0000000302030000, 0x0000000705070000, 0x0000000E0A0E0000, 0x0000001C141C0000,
            0x0000003828380000, 0x0000007050700000, 0x000000E0A0E00000, 0x000000C040C00000,
            0x0000030203000000, 0x0000070507000000, 0x00000E0A0E000000, 0x00001C141C000000,
            0x0000382838000000, 0x0000705070000000, 0x0000E0A0E0000000, 0x0000C040C0000000,
            0x0003020300000000, 0x0007050700000000, 0x000E0A0E00000000, 0x001C141C00000000,
            0x0038283800000000, 0x0070507000000000, 0x00E0A0E000000000, 0x00C040C000000000,
            0x0302030000000000, 0x0705070000000000, 0x0E0A0E0000000000, 0x1C141C0000000000,
            0x3828380000000000, 0x7050700000000000, 0xE0A0E00000000000, 0xC040C00000000000,
            0x0203000000000000, 0x0507000000000000, 0x0A0E000000000000, 0x141C000000000000,
            0x2838000000000000, 0x5070000000000000, 0xA0E0000000000000, 0x40C0000000000000
        };
        
        private static readonly BitBoard[] SlidingMoves = new BitBoard[87988];
        
        #endregion
        
        private readonly Board Board;
        private readonly Square From;
        
        public int Count => Moves.Count;
        private BitBoard Moves;

        public static void SetUp()
        {
            BlackMagicBitBoard.SetUp();
            GenerateSlidingMoves(Piece.Rook);
            GenerateSlidingMoves(Piece.Bishop);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static bool UnderAttack(Board board, Square sq, PieceColor by)
        {
            int s = (int)sq;
            BitBoard pawnAttack = by == PieceColor.White ? BlackPawnAttacks[s] : WhitePawnAttacks[s];
            if (pawnAttack & board.All(Piece.Pawn, by)) return true;
            if (KnightMoves[s] & board.All(Piece.Knight, by)) return true;
            BitBoard occupied = ~board.All(PieceColor.None);
            BitBoard queen = board.All(Piece.Queen, by);
                
            int mIndex = BlackMagicBitBoard.GetMagicIndex(Piece.Rook, occupied, sq);
            if (SlidingMoves[mIndex] & (queen | board.All(Piece.Rook, by))) return true;
                
            mIndex = BlackMagicBitBoard.GetMagicIndex(Piece.Bishop, occupied, sq);
            if (SlidingMoves[mIndex] & (queen | board.All(Piece.Bishop, by))) return true;
                
            return KingMoves[s] & board.All(Piece.King, by);
        }

        private static void GenerateSlidingMoves(Piece piece)
        {
            ((BitBoard, BitBoard, int)[], int) args = piece switch
            {
                Piece.Rook => (BlackMagicBitBoard.RookMagic, BlackMagicBitBoard.ROOK),
                Piece.Bishop => (BlackMagicBitBoard.BishopMagic, BlackMagicBitBoard.BISHOP),
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
            
            for (int h = 0; h < Board.UBOUND; h++)
            for (int v = 0; v < Board.UBOUND; v++) {
                BitBoard mask = ~args.Item1[v * 8 + h].Item2;
                Square sq = (Square)(v * 8 + h);
                
                BitBoard occupied = BitBoard.Default;
                while (true) {
                    BitBoard moves = BitBoard.Default;
                    foreach ((int dH, int dV) in deltas) {
                        int hI = h;
                        int vI = v;
                        
                        while (!occupied[vI * 8 + hI]) {
                            if (hI + dH is > 7 or < 0 || vI + dV is > 7 or < 0) break;
                            
                            hI += dH;
                            vI += dV;
                            moves |= (Square)(vI * 8 + hI);
                        }
                    }

                    SlidingMoves[BlackMagicBitBoard.GetMagicIndex(piece, occupied, sq)] = moves;

                    occupied = (occupied - mask) & mask;
                    if (occupied.Count == 0) break;
                }
                
                Console.WriteLine("Generated Black Magic Attack Table for [" + piece + "] at: " + sq);
            }
        }

        public MoveList(Board board, Square from, bool verify = true)
        {
            Board = board;
            From = from;
            Moves = BitBoard.Default;
            
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
                    LegalQueenMoveSet(color);
                    break;
                case Piece.King:
                    LegalKingMoveSet(color, !verify);
                    break;
                case Piece.Empty:
                default:
                    throw InvalidMoveLookupException.FromBoard(
                        board, 
                        "Cannot generate move for empty piece: " + from
                    );
            }
            
            if (!verify) return;
            VerifyMoves(color);
        }

        public MoveList(Board board, PieceColor color)
        {
            Board = board;
            Moves = BitBoard.Default;
            From = Square.Na;

            BitBoard colored = board.All(color);
            foreach (Square sq in colored) {
                MoveList moveList = new(board, sq);
                Moves |= moveList.Get();
            }
        }

        public BitBoard Get()
        {
            return Moves;
        }

        private void LegalPawnMoveSet(PieceColor color, bool checkMovesOnly = false)
        {
            PieceColor oppositeColor = Util.OppositeColor(color);
            BitBoard from = From;
            
            if (!checkMovesOnly) {
                // Normal
                // 1 Push
                Moves |= (color == PieceColor.White ? from << 8 : from >> 8) & Board.All(PieceColor.None);
                
                if ((int)From is > 7 and < 15 && Moves) {
                    // 2 Push
                    Moves |= color == PieceColor.White ? from << 16 : from >> 16;
                }

                Moves &= ~Board.All(oppositeColor);

                // En Passant
                if (Board.EnPassantTarget != Square.Na) {
                    BitBoard ep = Board.EnPassantTarget;
                    BitBoard target = color == PieceColor.White ? ep >> 8 : ep << 8;
                    BitBoard allOpposingPawns = Board.All(Piece.Pawn, oppositeColor);
                    
                    BitBoard pieceExistCheck = color == PieceColor.White ? 
                        BlackPawnAttacks[(int)Board.EnPassantTarget] : WhitePawnAttacks[(int)Board.EnPassantTarget];

                    if (target & allOpposingPawns && pieceExistCheck[From]) Moves |= ep;
                }
            }
            
            // Attack Moves
            BitBoard attack = color == PieceColor.White ? WhitePawnAttacks[(int)From] : BlackPawnAttacks[(int)From];

            Moves |= attack & Board.All(oppositeColor);
            Moves &= ~Board.All(color);
        }

        private void LegalRookMoveSet(PieceColor color)
        {
            int mIndex = BlackMagicBitBoard.GetMagicIndex(Piece.Rook, ~Board.All(PieceColor.None), From);
            Moves |= SlidingMoves[mIndex];
            Moves &= ~Board.All(color);
        }

        private void LegalKnightMoveSet(PieceColor color)
        {
            Moves |= KnightMoves[(int)From];
            Moves &= ~Board.All(color);
        }
        
        private void LegalBishopMoveSet(PieceColor color)
        {
            int mIndex = BlackMagicBitBoard.GetMagicIndex(Piece.Bishop, ~Board.All(PieceColor.None), From);
            Moves |= SlidingMoves[mIndex];
            Moves &= ~Board.All(color);
        }

        private void LegalQueenMoveSet(PieceColor color)
        {
            LegalRookMoveSet(color);
            LegalBishopMoveSet(color);
        }

        private void LegalKingMoveSet(PieceColor color, bool checkMovesOnly = false)
        {
            // Normal
            Moves |= KingMoves[(int)From];
            Moves &= ~Board.All(color);

            if (checkMovesOnly) return;

            PieceColor oppositeColor = Util.OppositeColor(color);
            if (UnderAttack(Board, From, oppositeColor)) return;

            // Castling
            (bool q, bool k) = Board.CastlingRight(color);
            if (q && !UnderAttack(Board, From - 1, oppositeColor)) {
                BitBoard path = new(BitBoard.Default)
                {
                    [From - 3] = true,
                    [From - 2] = true,
                    [From - 1] = true
                };
                BitBoard all = ~Board.All(PieceColor.None);
                if ((path & all) == BitBoard.Default) {
                    Moves |= From - 2;
                }
            }

            // ReSharper disable once InvertIf
            if (k && !UnderAttack(Board, From + 1, oppositeColor)) {
                BitBoard path = new(BitBoard.Default)
                {
                    [From + 2] = true,
                    [From + 1] = true
                };
                BitBoard all = ~Board.All(PieceColor.None);
                // ReSharper disable once InvertIf
                if ((path & all) == BitBoard.Default) {
                    Moves |= From + 2;
                }
            }
        }

        private void VerifyMoves(PieceColor color)
        {
            PieceColor oppositeColor = Util.OppositeColor(color);
            
            BitBoard verifiedMoves = BitBoard.Default;
            BitBoardMap originalState = Board.GetCurrentState;
            foreach (Square sq in Moves) {
                Board.Move(From, sq);

                BitBoard kingSafety = Board.KingLoc(color);
                if (!UnderAttack(Board, kingSafety, oppositeColor)) verifiedMoves[sq] = true;
                
                Board.UndoMove(ref originalState);
            }

            Moves = verifiedMoves;
        }

        public IEnumerator<Square> GetEnumerator()
        {
            return Moves.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}