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

        private static LoggingConfigurer CreateConfigurer()
        {
            var applicationName = Assembly.GetEntryAssembly().GetName().Name;
            if (NativeMethods.IsRunningAsService())
            {
                return new LoggingConfigurer(new ServiceLogOutputDescriptor(applicationName));
            }
            else
            {
                return new LoggingConfigurer(new ConsoleOutputDescriptor(applicationName, System.Console.Out, System.Console.Error));
            }
        }

        public static void Configure()
        {
            ConfigureWith(() =>
            {
                // ignore absent configuration:
                if (ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).GetSection("log4net") == null) return;

                XmlConfigurator.Configure();
            });
        }

        public static void ConfigureWith(Action action)
        {
            Debug.Assert(configurer == null);
            configurer = CreateConfigurer();
            action();
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
