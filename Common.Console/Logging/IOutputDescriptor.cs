using log4net.Appender;

namespace Bluewire.Common.Console.Logging
{
    public interface IOutputDescriptor
    {
        IAppender CreateStdOut();
        IAppender CreateStdErr();
        IAppender CreateDefaultLog();
    }
}