using System.IO;
using log4net.Appender;
using log4net.Layout;

namespace Bluewire.Common.Console.Logging
{
    public class ConsoleOutputDescriptor : OutputDescriptorBase
    {
        private readonly TextWriter stderr;

        public ConsoleOutputDescriptor(string applicationName, TextWriter stderr) : base(applicationName)
        {
            this.stderr = stderr;
        }

        public override IAppender CreateStdErr()
        {
            return new TextWriterAppender { Writer = this.stderr, Layout = Init(new PatternLayout("%message%newline")) };
        }
    }
}
