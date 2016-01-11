using System.Collections.Generic;
using System.Linq;

namespace Bluewire.Common.Console.Formatting
{
    class ColumnFormatter : IColumnFormatter
    {
        public Column Column { get; private set; }
        public int Index { get; private set; }

        public ColumnFormatter(Column column, int index)
        {
            Column = column;
            Index = index;
        }

        public IFormattedColumn Format(IEnumerable<IRow> cellsRowMajor)
        {
            var formatted = cellsRowMajor.Select(cs => cs.FormatCell(Index, Column));
            return new FormattedColumn(Column, formatted.ToArray());
        }
    }
}