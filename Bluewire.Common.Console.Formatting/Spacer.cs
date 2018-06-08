namespace Bluewire.Common.Console.Formatting
{
    public class Spacer : IColumn
    {
        private readonly string spacer;

        public Spacer(string spacer)
        {
            this.spacer = spacer;
            LeftPadding = 0;
        }

        public Spacer(int width)
        {
            this.spacer = "";
            LeftPadding = width;
        }

        public override string ToString()
        {
            return spacer;
        }

        public int LeftPadding { get; set; }
        public int RightPadding { get; set; }
    }
}
