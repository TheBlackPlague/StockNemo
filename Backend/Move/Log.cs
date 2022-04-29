#nullable enable
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using BetterConsoles.Tables;
using BetterConsoles.Tables.Builders;
using BetterConsoles.Tables.Configuration;
using BetterConsoles.Tables.Models;

namespace Backend.Move
{

    public class Log
    {

        private readonly List<(int, int)[]> MoveLog = new();

        public void WriteToLog((int, int) from, (int, int) to)
        {
            (int, int)[] currentMove =
            {
                from,
                to
            };
            MoveLog.Add(currentMove);
        }

        public int Count()
        {
            return MoveLog.Count;
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            int i = 1;
            foreach ((int, int)[] move in MoveLog) {
                builder.Append(i++ + ": " + Util.TupleToChessString(move[0]) + Util.TupleToChessString(move[1]));
            }

            return builder.ToString();
        }

        public Table DrawLogCli(int drawLimit = 3)
        {
            TableBuilder builder = new(new CellFormat(Alignment.Center));
            
            // Add move number column.
            builder.AddColumn("*");
            
            // Add column for color.
            builder.AddColumn(
                " White ", 
                new CellFormat(
                    foregroundColor: Color.Black, 
                    backgroundColor: Color.AntiqueWhite
                )
            );
            builder.AddColumn(
                " Black ", 
                new CellFormat(
                    foregroundColor: Color.Black, 
                    backgroundColor: Color.Coral
                )
            );

            Table table = builder.Build();

            int contentCount = MoveLog.Count / 2;
            for (int i = 0; i < drawLimit; i++) {
                if (i + 1 > MoveLog.Count) break;
                ICell[] cells = new ICell[3];
                cells[0] = new TableCell((contentCount - i).ToString());
                cells[1] = new TableCell("");
                cells[2] = new TableCell("");

                (int, int)[]? white = MoveLog.ElementAtOrDefault(MoveLog.Count - 1 - i);
                (int, int)[]? black = MoveLog.ElementAtOrDefault(MoveLog.Count - 2 - i);

                if (white != null) {
                    string whiteMove = Util.TupleToChessString(white[0]) + Util.TupleToChessString(white[1]);
                    cells[1] = new TableCell(
                        whiteMove,
                        new CellFormat(
                            foregroundColor: Color.Black,
                            backgroundColor: Color.AntiqueWhite
                        )
                    );
                }

                if (black != null) {
                    string blackMove = Util.TupleToChessString(black[0]) + Util.TupleToChessString(black[1]);
                    cells[2] = new TableCell(
                        blackMove, 
                        new CellFormat(
                            foregroundColor: Color.Black, 
                            backgroundColor: Color.Coral
                        )
                    );
                }

                table.AddRow(cells);
            }
            
            table.Config = TableConfig.Unicode();
            table.Config.hasInnerRows = true;

            return table;
        }

    }

}