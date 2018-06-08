using Bluewire.Common.Console.Environment;
using Bluewire.Common.Console.ThirdParty;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Repository.Hierarchy;

namespace Bluewire.Common.Console.Logging
{
    /// <summary>
    /// Logs to STDERR, messages only, at the configured level.
    /// </summary>
    public class SimpleConsoleLoggingPolicy : Log4NetLoggingPolicy, IReceiveOptions
    {
        public VerbosityList Verbosity { get; } = new VerbosityList();
        public string Pattern { get; set; } = "%message";
        public Level RootLevel { get; set; }

        void IReceiveOptions.ReceiveFrom(OptionSet options)
        {
            options.AddCollector(Verbosity);
        }

        protected override void Configure(IExecutionEnvironment environment, Hierarchy hierarchy)
        {
            if (hierarchy.Root.Appenders.Count == 0)
            {
                hierarchy.Root.AddAppender(CreateDefaultLogAppender());
            }
            ConfigureConsoleLogging(hierarchy, (Logger)Log.Console.Logger);
        }

        private void ConfigureConsoleLogging(Hierarchy hierarchy, Logger consoleLogger)
        {
            consoleLogger.Additivity = false;
            consoleLogger.ReentrancySafeSetLoggerLevel(Verbosity.CurrentVerbosity);

            var consoleAppender = hierarchy.GetAppenderByName("Console.STDERR")
                ?? CommonLogAppenders.CreateConsoleAppender("Console.STDERR", Pattern, Verbosity.CurrentVerbosity);
            consoleAppender.AddFilterIfPossible(new LevelRangeFilter { AcceptOnMatch = false });
            Log4NetHelper.Init(consoleAppender);
            if (!consoleLogger.Appenders.Contains(consoleAppender))
            {
                consoleLogger.AddAppender(consoleAppender);
            }
        }

        private IAppender CreateDefaultLogAppender()
        {
            var appender = CommonLogAppenders.CreateConsoleAppender("DefaultConsoleAppender", Pattern, Verbosity.CurrentVerbosity);
            return Log4NetHelper.Init(appender);
        }

        protected override void OnInitialConfiguration(IExecutionEnvironment environment, Hierarchy hierarchy)
        {
            // Set default root log level, if requested.
            if (RootLevel != null) hierarchy.Root.Level = RootLevel;
            base.OnInitialConfiguration(environment, hierarchy);
        }
    }
}
