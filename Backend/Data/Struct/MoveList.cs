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
            
            // First, we check if the square is being attacked by pawns.
            // To do this, we generate a reverse attack mask, letting our square act as a pawn and seeing if opposing
            // pawns exist on the squares in the mask. If so, our square can be attacked by pawns.
            BitBoard pawnAttack = by == PieceColor.White ? 
                AttackTable.BlackPawnAttacks[s] : AttackTable.WhitePawnAttacks[s];
            if (pawnAttack & board.All(Piece.Pawn, by)) return true;
            
            // Then, we check if the square is being attacked by knights.
            // To do this, we generate a reverse attack mask, letting our square act as a knight and seeing if opposing
            // knights exist on the squares in the mask. If so, our square can be attacked by knights.
            if (AttackTable.KnightMoves[s] & board.All(Piece.Knight, by)) return true;
            
            // Next, we check if the square is being attacked by sliding pieces.
            // To do this, first we need to find all occupied squares (by our and opposing pieces).
            BitBoard occupied = ~board.All(PieceColor.None);
            
            // We should check queen along with rook/bishop as queen moves are (rook moves | bishop moves).
            BitBoard queen = board.All(Piece.Queen, by);
                
            // Generate a reverse attack mask for rook, letting our square act as a rook and seeing if opposing rook or
            // queen exist on the squares in the mask. If so, our square can be attacked by either rook or queen.
            int mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Rook, occupied, sq);
            if (AttackTable.SlidingMoves[mIndex] & (queen | board.All(Piece.Rook, by))) return true;
                
            // Generate a reverse attack mask for bishop, letting our square act as a rook and seeing if opposing
            // bishop or queen exist on the squares in the mask. If so, our square can be attacked by either bishop
            // or queen.
            mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Bishop, occupied, sq);
            if (AttackTable.SlidingMoves[mIndex] & (queen | board.All(Piece.Bishop, by))) return true;
                
            // Lastly, we check if our square is being attacked by a king.
            // We generate a reverse attack mask, letting our square act as king and then check if there if opposing
            // king exists on the squares in the mask. If so, our square can be attacked by king.
            // Otherwise, this square is completely safe from all pieces.
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

            #region Normal moves.

            // Push pawn once.
            Moves |= (color == PieceColor.White ? from << 8 : from >> 8) & Board.All(PieceColor.None);
            
            if (((int)From is > 7 and < 16 || (int)From is > 47 and < 56) && Moves) {
                // If we are on the starting pawn position & the first pawn push was successful.
                // Push once more.
                Moves |= color == PieceColor.White ? from << 16 : from >> 16;
            }

            // Make sure our pushes are not stepping on to enemy pieces.
            // These are normal moves, not attack moves so we can't capture.
            Moves &= ~opposite;

            #endregion

            #region Attack moves.

            // En Passant
            if (Board.EnPassantTarget != Square.Na) {
                // If EP exists, then we need to check if a piece exists on square that's under attack from Ep, not
                // where we move to.
                Square epPieceSq = color == PieceColor.White ? 
                    Board.EnPassantTarget - 8 : Board.EnPassantTarget + 8;
                bool epTargetPieceExists = Board.All(Piece.Pawn, oppositeColor)[epPieceSq];
                
                // We need to check if a piece of ours exists to actually execute the EP.
                // We do this by running a reverse pawn mask, to determine whether a piece of ours is on the corner.
                BitBoard reverseCorner = color == PieceColor.White
                    ? AttackTable.BlackPawnAttacks[(int)Board.EnPassantTarget]
                    : AttackTable.WhitePawnAttacks[(int)Board.EnPassantTarget];
                
                // If both are true, then we can EP.
                if (epTargetPieceExists & reverseCorner[From]) Moves |= Board.EnPassantTarget;
            }
            
            // Attack Moves
            BitBoard attack = color == PieceColor.White ? 
                AttackTable.WhitePawnAttacks[(int)From] : AttackTable.BlackPawnAttacks[(int)From];

            // Make sure attacks are only on opposite pieces (and not on empty squares or squares occupied by
            // our pieces).
            Moves |= attack & opposite;
            
            #endregion
            
            // Make sure moves are only having moves on empty squares or attack moves for enemy squares.
            Moves &= ~Board.All(color);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LegalRookMoveSet(PieceColor color)
        {
            int mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Rook, ~Board.All(PieceColor.None), From);
            Moves |= AttackTable.SlidingMoves[mIndex];
            
            // Make sure moves are only having moves on empty squares or attack moves for enemy squares.
            Moves &= ~Board.All(color);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LegalKnightMoveSet(PieceColor color)
        {
            Moves |= AttackTable.KnightMoves[(int)From];
            
            // Make sure moves are only having moves on empty squares or attack moves for enemy squares.
            Moves &= ~Board.All(color);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LegalBishopMoveSet(PieceColor color)
        {
            int mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Bishop, ~Board.All(PieceColor.None), From);
            Moves |= AttackTable.SlidingMoves[mIndex];
            
            // Make sure moves are only having moves on empty squares or attack moves for enemy squares.
            Moves &= ~Board.All(color);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LegalQueenMoveSet(PieceColor color)
        {
            // We can generate queen moves by combining rook and bishop moves.
            LegalRookMoveSet(color);
            LegalBishopMoveSet(color);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void LegalKingMoveSet(PieceColor color)
        {
            #region Normal

            Moves |= AttackTable.KingMoves[(int)From];
            Moves &= ~Board.All(color);

            #endregion

            #region Castling

            // If enemy is attacking our king, we cannot castle.
            PieceColor oppositeColor = Util.OppositeColor(color);
            if (UnderAttack(Board, From, oppositeColor)) return;
            
            // Get castling rights.
            (bool q, bool k) = Board.CastlingRight(color);
            
            // Make sure castling close-path isn't under attack.
            if (q && !UnderAttack(Board, From - 1, oppositeColor)) {
                // Generate path of castle queen-side.
                BitBoard path = new(BitBoard.Default)
                {
                    [From - 3] = true,
                    [From - 2] = true,
                    [From - 1] = true
                };
                
                // If path is empty, we can castle.
                BitBoard all = ~Board.All(PieceColor.None);
                if ((path & all) == BitBoard.Default) {
                    Moves |= From - 2;
                }
            }

            // ReSharper disable once InvertIf
            // Make sure castling close-path isn't under attack.
            if (k && !UnderAttack(Board, From + 1, oppositeColor)) {
                // Generate path of castle king-side.
                BitBoard path = new(BitBoard.Default)
                {
                    [From + 2] = true,
                    [From + 1] = true
                };
                
                // If path is empty, we can castle.
                BitBoard all = ~Board.All(PieceColor.None);
                // ReSharper disable once InvertIf
                if ((path & all) == BitBoard.Default) {
                    Moves |= From + 2;
                }
            }
            
            #endregion
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void VerifyMoves(PieceColor color)
        {
            PieceColor oppositeColor = Util.OppositeColor(color);
            BitBoard verifiedMoves = BitBoard.Default;
            
            BitBoardIterator iterator = Moves.GetEnumerator();
            Square sq = iterator.Current;
            while (iterator.MoveNext()) {
                // To verify a move as legal from pseudo-legal, we first must make the move.
                RevertMove rv = Board.Move(From, sq);

                // Verify that our king isn't under attack.
                BitBoard kingSafety = Board.KingLoc(color);
                
                // If king isn't under attack, we add the move as it is a legal move.
                if (!UnderAttack(Board, kingSafety, oppositeColor)) verifiedMoves[sq] = true;

                // Regardless of whether we added, we must undo the move to get to the original state of board.
                Board.UndoMove(ref rv);

                sq = iterator.Current;
            }

            Moves = verifiedMoves;
        }

    }

}