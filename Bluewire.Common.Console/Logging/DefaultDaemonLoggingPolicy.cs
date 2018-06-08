using System.Diagnostics;
using System.IO;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Environment;
using Bluewire.Common.Console.Util;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Bluewire.Common.Console.Logging
{
    public class DefaultDaemonLoggingPolicy : Log4NetLoggingPolicy
    {
        /// <summary>
        /// The default base directory to use for logging. If null, this will
        /// be read from the appropriate *:LogDirectory AppSetting instead/
        /// </summary>
        public string LogDirectory { get; set; }
        /// <summary>
        /// Once the logging policy is initialised, this contains the actual
        /// base directory used for logging.
        /// </summary>
        public string InitialisedLogDirectory { get; private set; }

        protected override void Initialise(IExecutionEnvironment environment)
        {
            InitialisedLogDirectory = ConfigurationReader.Default.GetAbsolutePath(LogDirectory, DaemonRunnerSettings.GetLogDirectory(environment.ApplicationName));
            GlobalContext.Properties["LogDirectory"] = InitialisedLogDirectory;
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
            Debug.Assert(InitialisedLogDirectory != null);
            var appender = CommonLogAppenders.CreateLogFileAppender("DefaultLogAppender", Path.Combine(InitialisedLogDirectory, environment.ApplicationName));
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
