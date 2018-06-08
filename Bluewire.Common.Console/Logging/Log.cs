using log4net;

namespace Bluewire.Common.Console.Logging
{
    public class Log
    {
        public static ILog Console => LogManager.GetLogger("Console");
    }
}
