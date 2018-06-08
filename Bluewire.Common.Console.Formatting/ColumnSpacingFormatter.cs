using System.Collections.Generic;
using System.Linq;

namespace Bluewire.Common.Console.Formatting
{
    class ColumnSpacingFormatter : IColumnFormatter
    {
        private readonly string spacing;

        public ColumnSpacingFormatter(string spacing)
        {
            this.spacing = spacing;
        }

        public IFormattedColumn Format(IEnumerable<IRow> cellsRowMajor)
        {
            return new FixedColumn(spacing, cellsRowMajor.Select(c => c.FormatSpacer(spacing)));
        }
    }
}
