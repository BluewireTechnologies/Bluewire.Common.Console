namespace Bluewire.Common.Console.Formatting
{
    public interface IRow
    {
        AlignmentFormatter AlignCell(Column column);
        string FormatCell(int index, Column column);
        string FormatSpacer(string spacer);
    }
}