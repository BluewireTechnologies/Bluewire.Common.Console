using System.IO;
using log4net.Appender;
using log4net.Layout;

namespace Bluewire.Common.Console.Logging
{
    public class ConsoleOutputDescriptor : OutputDescriptorBase
    {
        private readonly TextWriter stdout;
        private readonly TextWriter stderr;

        public ConsoleOutputDescriptor(string applicationName, TextWriter stdout, TextWriter stderr) : base(applicationName)
        {
            this.stdout = stdout;
            this.stderr = stderr;
        }

        public override IAppender CreateStdOut()
        {
            return new TextWriterAppender { Writer = this.stdout, Layout = Init(new PatternLayout("%message%newline")) };
        }

        public override IAppender CreateStdErr()
        {
            return new TextWriterAppender { Writer = this.stderr, Layout = Init(new PatternLayout("%message%newline")) };
        }
    }
}