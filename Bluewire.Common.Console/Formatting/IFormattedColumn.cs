namespace Bluewire.Common.Console.Formatting
{
    interface IFormattedColumn
    {
        int Width { get; }
        string RenderCell(IRow row, int rowIndex);
    }
}
