using log4net.Appender;

namespace Bluewire.Common.Console.Logging
{
    public interface IOutputDescriptor
    {
        IAppender CreateStdErr();
        IAppender CreateDefaultLog();
    }
}
