using System;

namespace Bluewire.Common.Console.Environment
{
    [Serializable]
    public struct HostedEnvironmentDefinition
    {
        /// <summary>
        /// Effective name of the application being hosted.
        /// Assembly.GetEntryAssembly() is used to determine this in non-hosted environments.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Directory which will contain the STDOUT and STDERR log file.
        /// </summary>
        public string ConsoleLogDirectory { get; set; }
    }
}