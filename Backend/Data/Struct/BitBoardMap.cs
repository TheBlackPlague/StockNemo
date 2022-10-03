using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;
using Backend.Data.Template;
using Backend.Engine;

namespace Backend.Data.Struct;

public struct BitBoardMap
{
        
    private const string FEN_SPR = "/";

    private readonly BitBoard[][] Bb;
    private readonly byte[] PiecesAndColors;

    private BitBoard White;
    private BitBoard Black;

    public PieceColor ColorToMove;
        
    public byte WhiteKCastle;
    public byte WhiteQCastle;
    public byte BlackKCastle;
    public byte BlackQCastle;
        
    public Square EnPassantTarget;

    public ulong ZobristHash;

    public BitBoardMap(string boardFen, string turnData, string castlingData, string enPassantTargetData)
    {
        PiecesAndColors = new byte[64];
        for (int i = 0; i < 64; i++) PiecesAndColors[i] = 0x26;

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

        ColorToMove = turnData[0] == 'w' ? PieceColor.White : PieceColor.Black;
        WhiteKCastle = castlingData.Contains('K') ? (byte)0x1 : (byte)0x0;
        WhiteQCastle = castlingData.Contains('Q') ? (byte)0x2 : (byte)0x0;
        BlackKCastle = castlingData.Contains('k') ? (byte)0x4 : (byte)0x0;
        BlackQCastle = castlingData.Contains('q') ? (byte)0x8 : (byte)0x0;
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
        
        // Necessary to do two assignments to acknowledge struct is fully initialized.
        ZobristHash = 0;
        ZobristHash = Zobrist.Hash(ref this);
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
        ColorToMove = map.ColorToMove;
        EnPassantTarget = map.EnPassantTarget;
            
        PiecesAndColors = new byte[64];
        Bb = new BitBoard[2][];

        for (int i = 0; i < 2; i++) {
            Bb[i] = new BitBoard[6];
            Array.Copy(bb[i], Bb[i], 6);
        }
        Array.Copy(piecesAndColors, PiecesAndColors, 64);
        
        ZobristHash = map.ZobristHash;
    }

    public (Piece, PieceColor) this[Square sq]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            byte r = PiecesAndColors.AA((int)sq);
            return ((Piece)(r & 0xF), (PieceColor)(r >> 4));
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
        get => Bb.DJAA((int)color, (int)piece);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Piece PieceOnly(Square sq) => (Piece)(PiecesAndColors.AA((int)sq) & 0xF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PieceColor ColorOnly(Square sq) => (PieceColor)(PiecesAndColors.AA((int)sq) >> 4);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Move<MoveType>(Square from, Square to) where MoveType : MoveUpdateType
    {
        (Piece pF, PieceColor cF) = this[from];
        (Piece pT, PieceColor cT) = this[to];
        Move<MoveType>(pF, cF, pT, cT, from, to);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Move<MoveType>(Piece pF, PieceColor cF, Piece pT, PieceColor cT, Square from, Square to)
        where MoveType : MoveUpdateType
    {
        if (pT != Piece.Empty) {
            // If moving to piece isn't empty, then we capture.
            Bb.DJAA((int)cT, (int)pT)[to] = false;
                
            // Remove from color bitboards.
            if (cT == PieceColor.White) {
                White[to] = false;
            } else {
                Black[to] = false;
            }
            
            // Update Zobrist.
            Zobrist.HashPiece(ref ZobristHash, pT, cT, to);
        }
        
        ref BitBoard edit = ref Bb.DJAA((int)cF, (int)pF);
        
        // We remove from original square.
        edit[from] = false;

        // Set at next square.
        edit[to] = true;

        // Make sure to update the pieces and colors.
        PiecesAndColors.AA((int)to) = PiecesAndColors.AA((int)from);
        PiecesAndColors.AA((int)from) = 0x26;

        // Update color bitboards.
        if (cF == PieceColor.White) {
            White[from] = false;
            White[to] = true;
        } else {
            Black[from] = false;
            Black[to] = true;
        }
        
        // Update Zobrist.
        Zobrist.HashPiece(ref ZobristHash, pF, cF, from);
        Zobrist.HashPiece(ref ZobristHash, pF, cF, to);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Empty<UpdateType>(Square sq) where UpdateType : MoveUpdateType
    {
        (Piece piece, PieceColor color) = this[sq];
        Empty<UpdateType>(piece, color, sq);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Empty<UpdateType>(Piece piece, PieceColor color, Square sq) where UpdateType : MoveUpdateType
    {
        // Remove from square.
        Bb.DJAA((int)color, (int)piece)[sq] = false;
            
        // Set empty in pieces and colors.
        PiecesAndColors.AA((int)sq) = 0x26;

        // Remove from color bitboards.
        if (color == PieceColor.White) {
            White[sq] = false;
        } else {
            Black[sq] = false;
        }
        
        // Update Zobrist.
        Zobrist.HashPiece(ref ZobristHash, piece, color, sq);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InsertPiece<UpdateType>(Piece piece, PieceColor color, Square sq) where UpdateType : MoveUpdateType
    {
        // Insert the piece at square.
        Bb.DJAA((int)color, (int)piece)[sq] = true;
            
        // Insert into color bitboards.
        if (color == PieceColor.White) {
            White[sq] = true;
        } else {
            Black[sq] = true;
        }
            
        // Set piece in pieces and colors.
        int offset = color == PieceColor.White ? 0x0 : 0x10;
        PiecesAndColors.AA((int)sq) = (byte)((int)piece | offset);
        
        // Update Zobrist.
        Zobrist.HashPiece(ref ZobristHash, piece, color, sq);
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