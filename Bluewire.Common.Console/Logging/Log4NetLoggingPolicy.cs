using System;
using System.IO;
using Bluewire.Common.Console.Environment;
using log4net;
using log4net.Config;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace Bluewire.Common.Console.Logging
{
    public abstract class Log4NetLoggingPolicy : LoggingPolicy
    {
        private LoggerRepositoryConfigurationChangedEventHandler loggerConfigurationChangedHandler;

        /// <summary>
        /// The file from which log4net's configuration should be read. If null, the
        /// application's default configuration file will be used.
        /// Default: null.
        /// </summary>
        public string Log4NetConfigurationFilePath { get; set; }

        protected override void Initialise(IExecutionEnvironment environment)
        {
            if (loggerConfigurationChangedHandler != null) throw new InvalidOperationException($"This {GetType()} instance has already been registered.");
            var hierarchy = Log4NetHelper.DefaultHierachy;
            if (hierarchy.Configured)
            {
                LogManager.GetLogger(GetType()).Warn("log4net has already been configured, possibly by an [assembly: XmlConfigurator()] attribute. This LoggingPolicy may not function properly.");
            }
            else
            {
                OnInitialConfiguration(environment, hierarchy);
            }
            loggerConfigurationChangedHandler = (s, e) => OnConfigurationChanged(environment, hierarchy);
            hierarchy.ConfigurationChanged += loggerConfigurationChangedHandler;
            Configure(environment, hierarchy);
            hierarchy.Configured = true;
        }

        protected virtual void OnConfigurationChanged(IExecutionEnvironment environment, Hierarchy hierarchy)
        {
            Configure(environment, hierarchy);
        }

        /// <summary>
        /// Called during initialisation, unless log4net has already been configured by other means prior.
        /// </summary>
        /// <remarks>
        /// Default behaviour is to load configuration from the appdomain's own config file, or the file path
        /// in Log4NetConfigurationFilePath.
        /// </remarks>
        protected virtual void OnInitialConfiguration(IExecutionEnvironment environment, Hierarchy hierarchy)
        {
            LoadConfigurationFromFile();
        }

        protected void LoadConfigurationFromFile()
        {
            if (Log4NetConfigurationFilePath == null)
            {
                if (!Log4NetHelper.HasLog4NetConfiguration()) return;
                XmlConfigurator.Configure();
            }
            else
            {
                if (!Log4NetHelper.HasLog4NetConfiguration(Log4NetConfigurationFilePath)) return;
                XmlConfigurator.Configure(new FileInfo(Log4NetConfigurationFilePath));
            }
        }

        protected abstract void Configure(IExecutionEnvironment environment, Hierarchy hierarchy);

        protected override void ShutDown()
        {
            // Can't sanely 'undo' this policy, so leave it to the hosting application to do any clean-up which it really requires.
            // We must remove our change-detection handler, though.
            var hierarchy = Log4NetHelper.DefaultHierachy;
            hierarchy.ConfigurationChanged -= loggerConfigurationChangedHandler;
            loggerConfigurationChangedHandler = null;
        }
    }
}
