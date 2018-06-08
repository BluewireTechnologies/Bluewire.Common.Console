using log4net.Appender;
using log4net.Core;
using log4net.Layout;

namespace Bluewire.Common.Console.Logging
{
    public class CommonLogAppenders
    {
        public static RollingFileAppender CreateLogFileAppender(string appenderName, string path)
        {
            return new RollingFileAppender
            {
                Name = appenderName,
                File = path,
                RollingStyle = RollingFileAppender.RollingMode.Date,
                StaticLogFileName = true,
                DatePattern = @"'.'yyyy-MM-dd'.log'",
                Layout = Log4NetHelper.Init(new PatternLayout("%date [%3thread] %-5level %logger %ndc - %message%newline")),
                LockingModel = new FileAppender.MinimalLock()
            };
        }

        public static TextWriterAppender CreateConsoleAppender(string appenderName, string pattern, Level verbosity)
        {
            return new TextWriterAppender
            {
                Name = appenderName,
                Writer = System.Console.Error,
                Layout = new PatternLayout($"{pattern}%newline"),
                Threshold = verbosity
            };
        }
    }
}
