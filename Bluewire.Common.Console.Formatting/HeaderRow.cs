namespace Bluewire.Common.Console.Formatting
{
    public class HeaderRow : IRow
    {
        public AlignmentFormatter AlignCell(Column column)
        {
            return column.HeaderAlignment;
        }

        public string FormatCell(int index, Column column)
        {
            return column.Heading;
        }

        public string FormatSpacer(string spacer)
        {
            return "";
        }
    }
}
