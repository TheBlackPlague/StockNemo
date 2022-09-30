using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Data.Move;
using Backend.Data.Template;
using Backend.Exception;

namespace Backend.Data.Struct;

public ref struct MoveList
{

    private const ulong WHITE_KING_CASTLE = 0x60;
    private const ulong BLACK_KING_CASTLE = WHITE_KING_CASTLE << 56;
    private const ulong WHITE_QUEEN_CASTLE = 0xE;
    private const ulong BLACK_QUEEN_CASTLE = WHITE_QUEEN_CASTLE << 56;

    private readonly Board Board;
    private readonly Square From;
    private readonly BitBoard Hv;
    private readonly BitBoard D;
    private readonly BitBoard C;
    public int Count => Moves.Count;
    public BitBoard Moves { get; private set; }
    public bool Promotion { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static bool UnderAttack(Board board, Square sq, PieceColor by)
    {
        int s = (int)sq;
            
        // First, we check if the square is being attacked by pawns.
        // To do this, we generate a reverse attack mask, letting our square act as a pawn and seeing if opposing
        // pawns exist on the squares in the mask. If so, our square can be attacked by pawns.
        BitBoard pawnAttack = by == PieceColor.White ? 
            AttackTable.BlackPawnAttacks.AA(s) : AttackTable.WhitePawnAttacks.AA(s);
        if (pawnAttack & board.All(Piece.Pawn, by)) return true;
            
        // Then, we check if the square is being attacked by knights.
        // To do this, we generate a reverse attack mask, letting our square act as a knight and seeing if opposing
        // knights exist on the squares in the mask. If so, our square can be attacked by knights.
        if (AttackTable.KnightMoves.AA(s) & board.All(Piece.Knight, by)) return true;
            
        // Next, we check if the square is being attacked by sliding pieces.
        // To do this, first we need to find all occupied squares (by our and opposing pieces).
        BitBoard occupied = ~board.All(PieceColor.None);
            
        // We should check queen along with rook/bishop as queen moves are (rook moves | bishop moves).
        BitBoard queen = board.All(Piece.Queen, by);
                
        // Generate a reverse attack mask for rook, letting our square act as a rook and seeing if opposing rook or
        // queen exist on the squares in the mask. If so, our square can be attacked by either rook or queen.
        int mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Rook, occupied, sq);
        if (AttackTable.SlidingMoves.AA(mIndex) & (queen | board.All(Piece.Rook, by))) return true;
                
        // Generate a reverse attack mask for bishop, letting our square act as a rook and seeing if opposing
        // bishop or queen exist on the squares in the mask. If so, our square can be attacked by either bishop
        // or queen.
        mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Bishop, occupied, sq);
        if (AttackTable.SlidingMoves.AA(mIndex) & (queen | board.All(Piece.Bishop, by))) return true;
                
        // Lastly, we check if our square is being attacked by a king.
        // We generate a reverse attack mask, letting our square act as king and then check if there if opposing
        // king exists on the squares in the mask. If so, our square can be attacked by king.
        // Otherwise, this square is completely safe from all pieces.
        return AttackTable.KingMoves.AA(s) & board.All(Piece.King, by);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static (BitBoard, bool) CheckBitBoard(Board board, Square sq, PieceColor by)
    {
        int count = 0;
        BitBoard checks = BitBoard.Default;
        int s = (int)sq;
        
        // First we generate a pawn check.
        BitBoard pawnAttack = by == PieceColor.White ? 
            AttackTable.BlackPawnAttacks.AA(s) : AttackTable.WhitePawnAttacks.AA(s);
        BitBoard pawnCheck = pawnAttack & board.All(Piece.Pawn, by);
        
        // Next, we generate a knight check.
        BitBoard knightCheck = AttackTable.KnightMoves.AA(s) & board.All(Piece.Knight, by);
        
        // For sliding pieces, we use a BitBoard of all pieces.
        BitBoard occupied = ~board.All(PieceColor.None);

        // We will reference the queen along with rooks and bishops for the checks.
        BitBoard queen = board.All(Piece.Queen, by);
        
        // Now, we generate a rook or queen (straight only) check.
        int mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Rook, occupied, sq);
        BitBoard rookQueenCheck = AttackTable.SlidingMoves.AA(mIndex) & (queen | board.All(Piece.Rook, by));
        
        // Next, we generate a bishop or queen (diagonal only) check.
        mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Bishop, occupied, sq);
        BitBoard bishopQueenCheck = AttackTable.SlidingMoves.AA(mIndex) & (queen | board.All(Piece.Bishop, by));
        
        if (pawnCheck) {
            // If there is a pawn check, we must add it to the checks and raise the check count.
            checks |= pawnCheck;
            count++;
        } else if (knightCheck) {
            // Otherwise, if there is a knight check, we must add it to the checks and raise the check count.
            checks |= knightCheck;
            count++;
        }

        if (rookQueenCheck) {
            // If there is a rook-queen check hit, we must add in the checks as well as the path from square to
            // rook or queen.
            Square rqSq = rookQueenCheck;
            
            checks |= UtilityTable.Between.AA(s)[(int)rqSq] | rqSq;
            count++;
            
            // In the case where pawn promotes to queen or rook, we have a secondary check as well.
            if (rookQueenCheck.Count > 1) count++;
        }
        
        // ReSharper disable once InvertIf
        if (bishopQueenCheck) {
            // If there is a bishop-queen check hit, we must add in the checks as well as the path from square to
            // bishop or queen.
            Square bqSq = bishopQueenCheck;
            
            checks |= UtilityTable.Between.AA(s)[(int)bqSq] | bqSq;
            count++;
        }
        
        if (checks == BitBoard.Default) checks = BitBoard.Filled;

        return (checks, count > 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static (BitBoard, BitBoard) PinBitBoards(Board board, Square sq, PieceColor us, PieceColor by)
    {
        int s = (int)sq;
        
        // Unlike for all other boards and checks, we don't use a fully occupied board. We want our paths to go through
        // our pieces so we only consider board occupied by opposing.
        BitBoard byBoard = board.All(by);

        // We will reference the queen along with rooks and bishops for the checks.
        BitBoard queen = board.All(Piece.Queen, by);
        
        // First, we generate all rook / queen (straight only) attacks.
        int mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Rook, byBoard, sq);
        BitBoard rookQueenCheck = AttackTable.SlidingMoves.AA(mIndex) & (queen | board.All(Piece.Rook, by));
        
        // Next, we generate all bishop / queen (diagonal only) attacks.
        mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Bishop, byBoard, sq);
        BitBoard bishopQueenCheck = AttackTable.SlidingMoves.AA(mIndex) & (queen | board.All(Piece.Bishop, by));

        BitBoard horizontalVerticalPin = BitBoard.Default;
        BitBoard diagonalPin = BitBoard.Default;

        // Iterate over the rooks and queens (pinning straight).
        BitBoardIterator rookQueenIterator = rookQueenCheck.GetEnumerator();

        Square rqSq = rookQueenIterator.Current;
        while (rookQueenIterator.MoveNext()) {
            int rqS = (int)rqSq;
            BitBoard possiblePin = UtilityTable.Between.AA(s)[rqS] | rqSq;

            if ((possiblePin & board.All(us)).Count == 1) horizontalVerticalPin |= possiblePin;
            
            // Next square iteration.
            rqSq = rookQueenIterator.Current;
        }
        
        // Iterate over the bishops and queens (pinning diagonally).
        BitBoardIterator bishopQueenIterator = bishopQueenCheck.GetEnumerator();

        Square bqSq = bishopQueenIterator.Current;
        while (bishopQueenIterator.MoveNext()) {
            int bqS = (int)bqSq;
            BitBoard possiblePin = UtilityTable.Between.AA(s)[bqS] | bqSq;

            if ((possiblePin & board.All(us)).Count == 1) diagonalPin |= possiblePin;
            
            // Next square iteration.
            bqSq = bishopQueenIterator.Current;
        }

        return (horizontalVerticalPin, diagonalPin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MoveList WithoutProvidedPins(Board board, Square from)
    {
        (Piece piece, PieceColor color) = board.At(from);
        PieceColor oppositeColor = color.OppositeColor();
        
        Square kingSq = board.KingLoc(color);
        (BitBoard horizontalVertical, BitBoard diagonal) = PinBitBoards(board, kingSq, color, oppositeColor);
        (BitBoard checks, bool doubleChecked) = CheckBitBoard(board, kingSq, oppositeColor);
        return new MoveList(board, from, piece, color, ref horizontalVertical, ref diagonal, ref checks, doubleChecked);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public MoveList(Board board, Square from, Piece piece, PieceColor color, ref BitBoard horizontalVertical, 
        ref BitBoard diagonal, ref BitBoard checks, bool doubleChecked)
    {
        Board = board;
        From = from;
        Moves = BitBoard.Default;
        Promotion = false;
        Hv = horizontalVertical;
        D = diagonal;
        C = checks;

        // If we're double-checked (discovered check + normal check), only the king can move. Thus, we can return
        // early here.
        if (doubleChecked && piece != Piece.King) return;
        
        // Generate Legal Moves
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
    }
    
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public MoveList(Board board, Square from, Piece piece, PieceColor color, ref BitBoard horizontalVertical, 
        ref BitBoard diagonal, ref BitBoard checks)
    {
        Board = board;
        From = from;
        Moves = BitBoard.Default;
        Promotion = false;
        Hv = horizontalVertical;
        D = diagonal;
        C = checks;
        
        // Generate Legal Moves
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
    }
    
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public MoveList(Board board, Square from, ref BitBoard horizontalVertical, 
        ref BitBoard diagonal, ref BitBoard checks)
    {
        Board = board;
        From = from;
        Moves = BitBoard.Default;
        Promotion = false;
        Hv = horizontalVertical;
        D = diagonal;
        C = checks;
    }

    public MoveList(Board board, PieceColor color)
    {
        Board = board;
        Moves = BitBoard.Default;
        From = Square.Na;
        Promotion = false;
        Hv = BitBoard.Default;
        D = BitBoard.Default;
        C = BitBoard.Default;

        BitBoard colored = board.All(color);
        foreach (Square sq in colored) {
            MoveList moveList = WithoutProvidedPins(board, sq);
            Moves |= moveList.Moves;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void LegalPawnMoveSetCapture(PieceColor color)
    {
        if (Hv[From]) {
            // If pawn is horizontally pinned, then we have no moves.
            return;
        }
        
        PieceColor oppositeColor = color.OppositeColor();
        BitBoard opposite = Board.All(oppositeColor);
        Square epPieceSq = Square.Na;
        
        #region Promotion Flag

        // If we're at rank 7 for white or rank 1 for black, we should set the promotion flag to true.
        // It is important to set it earlier rather than later, because if there is a diagonal pin capture
        // leading to a promotion, we must make sure to record that as 4 moves.
        // FEN: 2q5/1Pp5/K2p4/7r/6Pk/8/8/1R6 w - -
        Promotion = color == PieceColor.White && From is > Square.H6 and < Square.A8 || 
                    color == PieceColor.Black && From is > Square.H1 and < Square.A3;

        #endregion
        
        #region Attack moves

        // En Passant.
        if (Board.EnPassantTarget != Square.Na) {
            // If EP exists, then we need to check if a piece exists on square that's under attack from Ep, not
            // where we move to.
            epPieceSq = color == PieceColor.White ? 
                Board.EnPassantTarget - 8 : Board.EnPassantTarget + 8;
            bool epTargetPieceExists = Board.All(Piece.Pawn, oppositeColor)[epPieceSq];
                
            // We need to check if a piece of ours exists to actually execute the EP.
            // We do this by running a reverse pawn mask, to determine whether a piece of ours is on the corner.
            BitBoard reverseCorner = color == PieceColor.White
                ? AttackTable.BlackPawnAttacks[(int)Board.EnPassantTarget]
                : AttackTable.WhitePawnAttacks[(int)Board.EnPassantTarget];
                
            if (epTargetPieceExists & reverseCorner[From]) {
                // If both the enemy EP piece and our piece that can theoretically EP exist...
                Moves |= Board.EnPassantTarget;
            }
        }
        
        // Attack Moves.
        BitBoard attack = color == PieceColor.White ? 
            AttackTable.WhitePawnAttacks.AA((int)From) : AttackTable.BlackPawnAttacks.AA((int)From);

        // Make sure attacks are only on opposite pieces (and not on empty squares or squares occupied by
        // our pieces).
        Moves |= attack & opposite & C;

        if (D[From]) {
            // If pawn is pinned diagonally, we can only do attacks and EP on the pin.
            Moves &= D & C;
            return;
        }
            
        #endregion
        
        #region Special EP case

        // ReSharper disable once InvertIf
        if (epPieceSq != Square.Na) {
            // If the pawn isn't pinned diagonally or horizontally/vertically, we must do one final check for EP:
            // In the rare EP-pin position: 8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -
            // If we do EP here, our king can be attacked by rook.
            // This is known as being pinned through a piece and only happens for EP, thus we must actually EP and see
            // if our king is under attacked.
            
            Board.RemovePiece<Normal>(Piece.Pawn, color, From);
            Board.RemovePiece<Normal>(Piece.Pawn, oppositeColor, epPieceSq);
            Board.InsertPiece<Normal>(Piece.Pawn, color, Board.EnPassantTarget);
            
            Square kingSq = Board.KingLoc(color);
            
            // If our king is under attack, it means the pawn was pinned through a piece and the removal of that piece
            // caused a discovered pin. Thus, we must remove it from our legal moves.
            if (UnderAttack(Board, kingSq, oppositeColor)) Moves &= ~(1UL << (int)Board.EnPassantTarget);
            
            Board.InsertPiece<Normal>(Piece.Pawn, color, From);
            Board.InsertPiece<Normal>(Piece.Pawn, oppositeColor, epPieceSq);
            Board.RemovePiece<Normal>(Piece.Pawn, color, Board.EnPassantTarget);

            // In the case that the EP piece isn't in our checks during a check, we shouldn't EP.
            if (Moves[Board.EnPassantTarget] && !C[epPieceSq]) Moves &= ~(1UL << (int)Board.EnPassantTarget);
        }

        #endregion
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void LegalPawnMoveSet(PieceColor color)
    {
        PieceColor oppositeColor = color.OppositeColor();
        BitBoard from = From;
        BitBoard opposite = Board.All(oppositeColor);
        Square epPieceSq = Square.Na;

        #region Promotion Flag

        // If we're at rank 7 for white or rank 1 for black, we should set the promotion flag to true.
        // It is important to set it earlier rather than later, because if there is a diagonal pin capture
        // leading to a promotion, we must make sure to record that as 4 moves.
        // FEN: 2q5/1Pp5/K2p4/7r/6Pk/8/8/1R6 w - -
        Promotion = color == PieceColor.White && From is > Square.H6 and < Square.A8 || 
                    color == PieceColor.Black && From is > Square.H1 and < Square.A3;

        #endregion

        #region Attack moves

        // En Passant.
        if (Board.EnPassantTarget != Square.Na) {
            // If EP exists, then we need to check if a piece exists on square that's under attack from Ep, not
            // where we move to.
            epPieceSq = color == PieceColor.White ? 
                Board.EnPassantTarget - 8 : Board.EnPassantTarget + 8;
            bool epTargetPieceExists = Board.All(Piece.Pawn, oppositeColor)[epPieceSq];
                
            // We need to check if a piece of ours exists to actually execute the EP.
            // We do this by running a reverse pawn mask, to determine whether a piece of ours is on the corner.
            BitBoard reverseCorner = color == PieceColor.White
                ? AttackTable.BlackPawnAttacks[(int)Board.EnPassantTarget]
                : AttackTable.WhitePawnAttacks[(int)Board.EnPassantTarget];
                
            if (epTargetPieceExists & reverseCorner[From]) {
                // If both the enemy EP piece and our piece that can theoretically EP exist...
                Moves |= Board.EnPassantTarget;
            }
        }
        
        // Attack Moves.
        BitBoard attack = color == PieceColor.White ? 
            AttackTable.WhitePawnAttacks.AA((int)From) : AttackTable.BlackPawnAttacks.AA((int)From);

        // Make sure attacks are only on opposite pieces (and not on empty squares or squares occupied by
        // our pieces).
        Moves |= attack & opposite & C;

        if (D[From]) {
            // If pawn is pinned diagonally, we can only do attacks and EP on the pin.
            Moves &= D & C;
            return;
        }
            
        #endregion
        
        #region Normal moves
        
        BitBoard pushes = BitBoard.Default;

        // Push pawn once.
        pushes |= (color == PieceColor.White ? from << 8 : from >> 8) & Board.All(PieceColor.None);
            
        if (From is > Square.H1 and < Square.A3 or > Square.H6 and < Square.A8 && pushes) {
            // If we are on the starting pawn position & the first pawn push was successful.
            // Push once more.
            pushes |= color == PieceColor.White ? from << 16 : from >> 16;
        }

        // Make sure our pushes are not stepping on to enemy pieces.
        // These are normal moves, not attack moves so we can't capture.
        pushes &= ~opposite & ~Board.All(color);
        
        Moves |= pushes & C;

        if (Hv[From]) {
            // If pawn is horizontally pinned, then we have no moves.
            // However, if pawn is vertically pinned, then we can at least do pushes.
            Moves &= Hv;
            return;
        }
        
        #endregion

        #region Special EP case

        // ReSharper disable once InvertIf
        if (epPieceSq != Square.Na) {
            // If the pawn isn't pinned diagonally or horizontally/vertically, we must do one final check for EP:
            // In the rare EP-pin position: 8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -
            // If we do EP here, our king can be attacked by rook.
            // This is known as being pinned through a piece and only happens for EP, thus we must actually EP and see
            // if our king is under attacked.
            
            Board.RemovePiece<Normal>(Piece.Pawn, color, From);
            Board.RemovePiece<Normal>(Piece.Pawn, oppositeColor, epPieceSq);
            Board.InsertPiece<Normal>(Piece.Pawn, color, Board.EnPassantTarget);
            
            Square kingSq = Board.KingLoc(color);
            
            // If our king is under attack, it means the pawn was pinned through a piece and the removal of that piece
            // caused a discovered pin. Thus, we must remove it from our legal moves.
            if (UnderAttack(Board, kingSq, oppositeColor)) Moves &= ~(1UL << (int)Board.EnPassantTarget);
            
            Board.InsertPiece<Normal>(Piece.Pawn, color, From);
            Board.InsertPiece<Normal>(Piece.Pawn, oppositeColor, epPieceSq);
            Board.RemovePiece<Normal>(Piece.Pawn, color, Board.EnPassantTarget);

            // In the case that the EP piece isn't in our checks during a check, we shouldn't EP.
            if (Moves[Board.EnPassantTarget] && !C[epPieceSq]) Moves &= ~(1UL << (int)Board.EnPassantTarget);
        }

        #endregion
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LegalRookMoveSet(PieceColor color)
    {
        // If rook is diagonally pinned, it has no moves.
        if (D[From]) return;
        
        // Calculate pseudo-legal moves within check board.
        int mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Rook, ~Board.All(PieceColor.None), From);
        Moves |= AttackTable.SlidingMoves.AA(mIndex) & ~Board.All(color) & C;

        // If rook is horizontally or vertically pinned, it can only move within the pin.
        if (Hv[From]) Moves &= Hv;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LegalKnightMoveSet(PieceColor color)
    {
        if (Hv[From] || D[From]) return;

        Moves |= AttackTable.KnightMoves.AA((int)From) & ~Board.All(color) & C;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LegalBishopMoveSet(PieceColor color)
    {
        // If bishop is horizontally or vertically pinned, it has no moves.
        if (Hv[From]) return;

        // Calculate pseudo-legal moves within check board.
        int mIndex = BlackMagicBitBoardFactory.GetMagicIndex(Piece.Bishop, ~Board.All(PieceColor.None), From);
        Moves |= AttackTable.SlidingMoves.AA(mIndex) & ~Board.All(color) & C;

        // If bishop is diagonally pinned, it can only move within the pin.
        if (D[From]) Moves &= D;
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

        BitBoard kingMoves = AttackTable.KingMoves.AA((int)From);
        kingMoves &= ~Board.All(color);
        
        // If we have no king moves, we can return earlier and avoiding having to check if the moves are legal
        // or not by removing the king.
        if (!kingMoves) return;

        PieceColor oppositeColor = color.OppositeColor();
        BitBoardIterator kingMovesIterator = kingMoves.GetEnumerator();
        Square move = kingMovesIterator.Current;
        Board.RemovePiece<Normal>(Piece.King, color, From);
        while (kingMovesIterator.MoveNext()) {
            if (UnderAttack(Board, move, oppositeColor)) kingMoves[move] = false;
            
            // Next square iteration.
            move = kingMovesIterator.Current;
        }
        Board.InsertPiece<Normal>(Piece.King, color, From);

        Moves |= kingMoves;

        #endregion

        #region Castling

        // If enemy is attacking our king, we cannot castle.
        if (UnderAttack(Board, From, oppositeColor)) return;
            
        // Get castling rights.
        (byte q, byte k) = Board.CastlingRight(color);
            
        // Make sure castling close-path isn't under attack.
        if (q != 0x0 && kingMoves[From - 1] && !UnderAttack(Board, From - 2, oppositeColor)) {
            // Generate path of castle queen-side.
            BitBoard path = color == PieceColor.White ? WHITE_QUEEN_CASTLE : BLACK_QUEEN_CASTLE;
                
            // If path is empty, we can castle.
            BitBoard all = ~Board.All(PieceColor.None);
            if ((path & all) == BitBoard.Default) {
                Moves |= From - 2;
            }
        }

        // ReSharper disable once InvertIf
        // Make sure castling close-path isn't under attack.
        if (k != 0x0 && kingMoves[From + 1] && !UnderAttack(Board, From + 2, oppositeColor)) {
            // Generate path of castle king-side.
            BitBoard path = color == PieceColor.White ? WHITE_KING_CASTLE : BLACK_KING_CASTLE;
                
            // If path is empty, we can castle.
            BitBoard all = ~Board.All(PieceColor.None);
            // ReSharper disable once InvertIf
            if ((path & all) == BitBoard.Default) {
                Moves |= From + 2;
            }
        }
            
        #endregion
    }

}