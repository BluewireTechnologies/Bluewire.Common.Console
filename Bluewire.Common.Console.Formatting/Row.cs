using System.Collections.Generic;
using System.Linq;

namespace Bluewire.Common.Console.Formatting
{
    public class Row : IRow
    {
        private readonly IEnumerable<object> cells;

        public AlignmentFormatter AlignCell(Column column)
        {
            return column.CellAlignment;
        }

        public Row(IEnumerable<object> cells)
        {
            this.cells = cells.ToArray();
        }

        public string FormatCell(int index, Column column)
        {
            return column.FormatValue(cells.ElementAtOrDefault(index));
        }


        public string FormatSpacer(string spacer)
        {
            return spacer;
        }
    }
}
