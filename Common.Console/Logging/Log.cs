using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using log4net;
using log4net.Config;
using log4net.Core;

namespace Bluewire.Common.Console.Logging
{
    public class Log
    {
        private static LoggingConfigurer configurer;

        private static OutputDescriptorBase CreateDescriptor()
        {
            var applicationName = Assembly.GetEntryAssembly().GetName().Name;
            if (NativeMethods.IsRunningAsService())
            {
                return new ServiceLogOutputDescriptor(applicationName);
            }
            else
            {
                return new ConsoleOutputDescriptor(applicationName, System.Console.Out, System.Console.Error);
            }
        }

        /// <summary>
        /// Configure Log4Net from application's config file if possible.
        /// </summary>
        /// <param name="defaultLogFileRoot">Root directory used for logfiles written by the default logging behaviour. Does not affect configured logging. Defaults to the application directory.</param>
        public static void Configure(string defaultLogFileRoot = null)
        {
            ConfigureWith(c =>
            {
                c.SetLogRootDirectory(defaultLogFileRoot);

                // ignore absent configuration:
                if (HasLog4NetConfiguration(GetApplicationConfiguration()))
                {
                    XmlConfigurator.Configure();
                }
            });
        }

        internal static Configuration GetApplicationConfiguration()
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public static bool HasLog4NetConfiguration(Configuration configuration)
        {
            return (configuration.GetSection("log4net") != null);
        }

        public static void ConfigureWith(Action<IOutputDescriptorConfiguration> action)
        {
            Debug.Assert(configurer == null);
            var descriptor = CreateDescriptor();
            action(descriptor);
            configurer = new LoggingConfigurer(descriptor);
        }

        private static void AssertThatLoggingIsConfigured()
        {
            if (configurer == null) throw new InvalidOperationException("Logging has not yet been configured.");
        }

        public static void SetConsoleVerbosity(Level verbosity)
        {
            AssertThatLoggingIsConfigured();
            configurer.ConsoleVerbosity = verbosity;
        }

        public static ILog Console
        {
            get
            {
                AssertThatLoggingIsConfigured();
                return configurer.Console;
            }
        }
    }
}
