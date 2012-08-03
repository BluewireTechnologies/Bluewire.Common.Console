using System;

namespace Bluewire.Common.Console.Formatting
{
    public class Column : IColumn
    {
        private readonly string format;
        public string Heading { get; private set; }
        
        public Column(string heading) : this(heading, "{0}")
        {
        }

        public Column(string heading, string format)
        {
            this.format = format;
            Heading = heading;
            LeftPadding = 1;
            RightPadding = 1;
            CellAlignment = Alignment.Right;
            HeaderAlignment = Alignment.Centre;
        }

        private AlignmentFormatter headerAlignment;
        public AlignmentFormatter HeaderAlignment
        {
            get { return this.headerAlignment ?? CellAlignment; }
            set { this.headerAlignment = value; }
        }

        public AlignmentFormatter CellAlignment { get; set; }

        public int LeftPadding { get; set; }
        public int RightPadding { get; set; }
        
        public virtual string FormatValue(object value)
        {
            return String.Format(format, value);
        }
    }
}