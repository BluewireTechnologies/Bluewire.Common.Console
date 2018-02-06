using System.Collections.Generic;
using System.Linq;

namespace Bluewire.Common.Console.Formatting
{
    class FixedColumn : IFormattedColumn
    {
        private readonly IEnumerable<string> cells;

        public FixedColumn(string spacer, IEnumerable<string> cells)
        {
            Width = spacer.Length;
            this.cells = cells;
        }

        public int Width { get; private set; }

        public string RenderCell(IRow row, int rowIndex)
        {
            return Alignment.Centre(cells.ElementAt(rowIndex), Width);
        }
    }
}
