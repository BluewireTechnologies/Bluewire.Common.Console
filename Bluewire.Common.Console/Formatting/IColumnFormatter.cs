using System.Collections.Generic;

namespace Bluewire.Common.Console.Formatting
{
    interface IColumnFormatter
    {
        IFormattedColumn Format(IEnumerable<IRow> cellsRowMajor);
    }
}
