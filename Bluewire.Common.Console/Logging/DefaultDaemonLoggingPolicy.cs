using System.IO;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Environment;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Bluewire.Common.Console.Logging
{
    public class DefaultDaemonLoggingPolicy : Log4NetLoggingPolicy
    {
        public string LogDirectory { get; set; } = DaemonRunnerSettings.LogDirectory;

        protected override void Initialise(IExecutionEnvironment environment)
        {
            GlobalContext.Properties["LogDirectory"] = LogDirectory;
            base.Initialise(environment);
        }

        protected override void Configure(IExecutionEnvironment environment, Hierarchy hierarchy)
        {
            if (hierarchy.Root.Appenders.Count == 0)
            {
                hierarchy.Root.AddAppender(CreateDefaultLogAppender(environment));
            }
        }

        private IAppender CreateDefaultLogAppender(IExecutionEnvironment environment)
        {
            var appender = CommonLogAppenders.CreateLogFileAppender("DefaultLogAppender", Path.Combine(LogDirectory, environment.ApplicationName));
            return Log4NetHelper.Init(appender);
        }

        protected override void OnInitialConfiguration(IExecutionEnvironment environment, Hierarchy hierarchy)
        {
            // Set log threshold at WARN by default.
            hierarchy.Root.Level = Level.Warn;
            base.OnInitialConfiguration(environment, hierarchy);
        }
    }
}
