using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Data.Move;
using Backend.Exception;

namespace Backend.Data.Struct
{

    public ref struct MoveList
    {

        private readonly Board Board;
        private readonly Square From;

        public int Count => Moves.Count;
        private BitBoard Moves;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static bool UnderAttack(Board board, Square sq, PieceColor by)
        {
            int s = (int)sq;
            BitBoard pawnAttack = by == PieceColor.White ? 
                AttackTable.BlackPawnAttacks[s] : AttackTable.WhitePawnAttacks[s];
            if (pawnAttack & board.All(Piece.Pawn, by)) return true;
            if (AttackTable.KnightMoves[s] & board.All(Piece.Knight, by)) return true;
            BitBoard occupied = ~board.All(PieceColor.None);
            BitBoard queen = board.All(Piece.Queen, by);
                
            int mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Rook, occupied, sq);
            if (AttackTable.SlidingMoves[mIndex] & (queen | board.All(Piece.Rook, by))) return true;
                
            mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Bishop, occupied, sq);
            if (AttackTable.SlidingMoves[mIndex] & (queen | board.All(Piece.Bishop, by))) return true;
                
            return AttackTable.KingMoves[s] & board.All(Piece.King, by);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public MoveList(Board board, Square from, bool verify = true)
        {
            Board = board;
            From = from;
            Moves = BitBoard.Default;
            
            (Piece piece, PieceColor color) = Board.At(from);
            // Generate Pseudo-Legal Moves
            switch (piece) {
                case Piece.Pawn:
                    LegalPawnMoveSet(color);
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
                    LegalKingMoveSet(color);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Get() => Moves;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void LegalPawnMoveSet(PieceColor color)
        {
            PieceColor oppositeColor = Util.OppositeColor(color);
            BitBoard from = From;
            BitBoard opposite = Board.All(oppositeColor);
            
            if (true) {
                // Normal
                // 1 Push
                Moves |= (color == PieceColor.White ? from << 8 : from >> 8) & Board.All(PieceColor.None);
                
                if (((int)From is > 7 and < 16 || (int)From is > 47 and < 56) && Moves) {
                    // 2 Push
                    Moves |= color == PieceColor.White ? from << 16 : from >> 16;
                }

                Moves &= ~opposite;

                // En Passant
                if (Board.EnPassantTarget != Square.Na) {
                    Square epPieceSq = color == PieceColor.White ? 
                        Board.EnPassantTarget - 8 : Board.EnPassantTarget + 8;
                    bool epTargetPieceExists = Board.All(Piece.Pawn, oppositeColor)[epPieceSq];
                    BitBoard reverseCorner = color == PieceColor.White
                        ? AttackTable.BlackPawnAttacks[(int)Board.EnPassantTarget]
                        : AttackTable.WhitePawnAttacks[(int)Board.EnPassantTarget];
                    if (epTargetPieceExists & reverseCorner[From]) Moves |= Board.EnPassantTarget;
                }
            }
            
            // Attack Moves
            BitBoard attack = color == PieceColor.White ? AttackTable.WhitePawnAttacks[(int)From] : AttackTable.BlackPawnAttacks[(int)From];

            Moves |= attack & opposite;
            Moves &= ~Board.All(color);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LegalRookMoveSet(PieceColor color)
        {
            int mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Rook, ~Board.All(PieceColor.None), From);
            Moves |= AttackTable.SlidingMoves[mIndex];
            Moves &= ~Board.All(color);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LegalKnightMoveSet(PieceColor color)
        {
            Moves |= AttackTable.KnightMoves[(int)From];
            Moves &= ~Board.All(color);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LegalBishopMoveSet(PieceColor color)
        {
            int mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Bishop, ~Board.All(PieceColor.None), From);
            Moves |= AttackTable.SlidingMoves[mIndex];
            Moves &= ~Board.All(color);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LegalQueenMoveSet(PieceColor color)
        {
            LegalRookMoveSet(color);
            LegalBishopMoveSet(color);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void LegalKingMoveSet(PieceColor color)
        {
            // Normal
            Moves |= AttackTable.KingMoves[(int)From];
            Moves &= ~Board.All(color);

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

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void VerifyMoves(PieceColor color)
        {
            PieceColor oppositeColor = Util.OppositeColor(color);
            
            BitBoard verifiedMoves = BitBoard.Default;
            BitBoardIterator iterator = Moves.GetEnumerator();
            Square sq = iterator.Current;
            while (iterator.MoveNext()) {
                RevertMove rv = Board.Move(From, sq);

                BitBoard kingSafety = Board.KingLoc(color);
                if (!UnderAttack(Board, kingSafety, oppositeColor)) verifiedMoves[sq] = true;

                Board.UndoMove(ref rv);

                sq = iterator.Current;
            }

            // for (Square sq = iterator.Current; iterator.MoveNext(); sq = iterator.Current) {
            //     Board.Move(From, sq);
            //     
            //     BitBoard kingSafety = Board.KingLoc(color);
            //     if (!UnderAttack(Board, kingSafety, oppositeColor)) verifiedMoves[sq] = true;
            //     
            //     Board.UndoMove(ref originalState);
            // }

            // foreach (Square sq in Moves) {
            //     Board.Move(From, sq);
            //
            //     BitBoard kingSafety = Board.KingLoc(color);
            //     if (!UnderAttack(Board, kingSafety, oppositeColor)) verifiedMoves[sq] = true;
            //     
            //     Board.UndoMove(ref originalState);
            // }

            Moves = verifiedMoves;
        }

    }

}