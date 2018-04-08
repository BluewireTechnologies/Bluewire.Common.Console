using System;
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
        public const string LibraryNamespace = "Bluewire.Common.Console";

        /// <summary>
        /// Default location: [app-path]/logs/
        /// </summary>
        public static string GetLogDirectory(string applicationName) =>
            ConfigurationReader.Default.GetAbsolutePath(GetAppSetting(applicationName, "LogDirectory"), "logs")
                .EnsureSingleTrailing(Path.DirectorySeparatorChar);

        /// <summary>
        /// Default location: [LogDirectory]
        /// </summary>
        public static string GetConsoleLogDirectory(string applicationName) =>
            ConfigurationReader.Default.GetAbsolutePath(GetLogDirectory(applicationName), GetAppSetting(applicationName, "ConsoleLogDirectory"), "")
                .EnsureSingleTrailing(Path.DirectorySeparatorChar);

        /// <summary>
        /// Read AppSetting key from application's own namespace preferentially, falling back to the library namespace.
        /// </summary>
        private static string GetAppSetting(string applicationName, string key) =>
            ReadAppSettingValue(applicationName, key) ?? ReadAppSettingValue(LibraryNamespace, key);

        /// <summary>
        /// Read AppSetting key within the given namespace. Returns null if keyNamespace is empty.
        /// </summary>
        private static string ReadAppSettingValue(string keyNamespace, string key) =>
            String.IsNullOrEmpty(keyNamespace) ? null : ConfigurationManager.AppSettings[$"{keyNamespace}:{key}"];
    }
}
