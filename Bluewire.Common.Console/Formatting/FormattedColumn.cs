using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bluewire.Common.Console.Formatting
{
    class FormattedColumn : IEnumerable<string>, IFormattedColumn
    {
        private readonly IEnumerable<string> cells;

        public Column Column { get; private set; }
        public int Width { get; private set; }

        public FormattedColumn(Column column, IEnumerable<string> cells)
        {
            this.cells = cells.ToArray();
            Column = column;
            Width = cells.Max(c => c.Length);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return cells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string RenderCell(IRow row, int rowIndex)
        {
            var align = row.AlignCell(Column);
            return align(this.ElementAt(rowIndex), Width);
        }
    }
}