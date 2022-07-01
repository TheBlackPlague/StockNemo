using System.Drawing;
using System.Text.Json.Serialization;
using Backend;
using Backend.Data.Enum;
using Backend.Data.Struct;
using Backend.Engine;
using BetterConsoles.Core;
using BetterConsoles.Tables;
using BetterConsoles.Tables.Builders;
using BetterConsoles.Tables.Configuration;
using BetterConsoles.Tables.Models;

namespace Terminal;

internal class DisplayBoard : EngineBoard
{

    private BitBoard HighlightedMoves = BitBoard.Default;
    
    public new static DisplayBoard Default()
    {
        return FromFen(DEFAULT_FEN);
    }

    public new static DisplayBoard FromFen(string fen)
    {
        string[] parts = fen.Split(" ");
        return new DisplayBoard(parts[0], parts[1], parts[2], parts[3]);
    }

    private DisplayBoard(string boardData, string turnData, string castlingData, string enPassantTargetData) : 
        base(boardData, turnData, castlingData, enPassantTargetData) {}
    
    public void HighlightMoves(Square from)
    {
        MoveList moveList = MoveList.WithoutProvidedPins(this, from);
        HighlightedMoves = moveList.Moves;
    }
    
    public string ToString(bool flip = false)
    {
        string board = DrawBoardCli(flip).ToString().Trim(' ');
        string fen = "FEN: " + GenerateFen() + "\n";
        string hash = "Hash: " + $"{Map.ZobristHash:X}\n";
        string eval = "Evaluation: " + Evaluation.NormalEvaluation(this);
        return board + fen + hash + eval;
    }
    
    private Table DrawBoardCli(bool flip)
    {
        TableBuilder builder = new(new CellFormat(Alignment.Center));
            
        // Add rank column
        builder.AddColumn("*");
            
        // Add columns for files
        int hStart = flip ? UBOUND - 1 : 0;
        int hStop = flip ? LBOUND : UBOUND;
        int hDelta = flip ? -1 : 1;
        for (int h = hStart; h != hStop; h += hDelta) builder.AddColumn(((char)(65 + h)).ToString());

        Table table = builder.Build();

        int vStart = flip ? 0 : UBOUND - 1;
        int vStop = flip ? UBOUND : LBOUND;
        int vDelta = flip ? 1 : -1;
        
        for (int v = vStart; v != vStop; v += vDelta) {
            // Count: Rank column + Files columns (1 + 8)
            ICell[] cells = new ICell[UBOUND + 1];
            cells[0] = new TableCell((v + 1).ToString());
            for (int h = hStart; h != hStop; h += hDelta) {
                Square sq = (Square)(v * 8 + h);
                (Piece piece, PieceColor color) = Map[sq];
                string pieceRepresentation = piece switch
                {
                    Piece.Empty => "   ",
                    Piece.Knight => "N",
                    _ => piece.ToString()[0].ToString()
                };

                Color uiColor = color switch
                {
                    PieceColor.White => Color.AntiqueWhite,
                    PieceColor.Black => Color.Coral,
                    _ => Color.Gray
                };

                if (HighlightedMoves[sq]) {
                    uiColor = piece == Piece.Empty ? Color.Yellow : Color.Red;
                    if (piece == Piece.Empty && sq == EnPassantTarget) 
                        uiColor = Color.Red; 
                }

                // Set piece value for file
                cells[h + 1] = new TableCell(
                    pieceRepresentation,
                    new CellFormat(
                        fontStyle: FontStyleExt.Bold,
                        foregroundColor: Color.Black,
                        backgroundColor: uiColor,
                        alignment: Alignment.Center
                    )
                );
            }

            // Add rank row
            table.AddRow(cells);
        }
            
        table.Config = TableConfig.Unicode();
        table.Config.hasInnerRows = true;

        HighlightedMoves = BitBoard.Default;

        return table;
    }

}