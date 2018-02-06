using System.Configuration;
using System.IO;
using Bluewire.Common.Console.Util;

namespace Bluewire.Common.Console.Daemons
{
    /// <summary>
    /// Static configuration settings taken from the appdomain's configuration files.
    /// </summary>
    public static class DaemonRunnerSettings
    {
        /// <summary>
        /// Default location: [app-path]/logs/
        /// </summary>
        public static string LogDirectory =>
            ConfigurationReader.Default.GetAbsolutePath(ConfigurationManager.AppSettings["Bluewire.Common.Console:LogDirectory"], "logs")
                .EnsureSingleTrailing(Path.DirectorySeparatorChar);

        /// <summary>
        /// Default location: [LogDirectory]
        /// </summary>
        public static string ConsoleLogDirectory =>
            ConfigurationReader.Default.GetAbsolutePath(LogDirectory, ConfigurationManager.AppSettings["Bluewire.Common.Console:ConsoleLogDirectory"], "")
                .EnsureSingleTrailing(Path.DirectorySeparatorChar);
    }
}
