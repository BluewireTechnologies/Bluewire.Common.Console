namespace Bluewire.Common.Console.Formatting
{
    public class EmptyRow : IRow
    {
        public AlignmentFormatter AlignCell(Column column)
        {
            return column.CellAlignment;
        }

        public string FormatSpacer(string spacer)
        {
            return "";
        }

        public string FormatCell(int index, Column column)
        {
            return "";
        }
    }
}