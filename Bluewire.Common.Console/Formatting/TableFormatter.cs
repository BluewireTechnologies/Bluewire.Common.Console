using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Bluewire.Common.Console.Formatting
{
    public class TableFormatter
    {
        private readonly IEnumerable<IColumnFormatter> columns;

        public TableFormatter(IEnumerable<IColumn> columns)
        {
            this.columns = GenerateColumnFormatters(columns).ToArray();
        }

        private static IEnumerable<IColumnFormatter> GenerateColumnFormatters(IEnumerable<IColumn> columns)
        {
            var previousColumnPadding = 0;
            var index = 0;
            foreach (var column in columns)
            {
                var padding = Math.Max(previousColumnPadding, column.LeftPadding);
                if (padding > 0) yield return new ColumnSpacingFormatter("".PadLeft(padding));
                if (column is Column)
                {
                    yield return new ColumnFormatter((Column) column, index);
                    index++;
                }
                else if (column is Spacer)
                {
                    yield return new ColumnSpacingFormatter(column.ToString());
                }
                previousColumnPadding = column.RightPadding;
            }
        }

        public string[] Format(IEnumerable<IRow> rows)
        {
            var formattedCellsColumnMajor = columns.Select(c => c.Format(rows)).ToArray();

            var renderedCells = rows.Select((r, ri) => formattedCellsColumnMajor.Select(c => c.RenderCell(r, ri)));

            return renderedCells.Select(CoalesceRow).ToArray();
        }

        private static string CoalesceRow(IEnumerable<string> cells)
        {
            return String.Join("", cells.ToArray());
        }

    }
}
