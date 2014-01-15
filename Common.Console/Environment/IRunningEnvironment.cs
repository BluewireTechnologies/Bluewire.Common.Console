using System;
using System.Reflection;

namespace Bluewire.Common.Console.Environment
{
    [Serializable]
    public struct HostedEnvironmentDefinition
    {
        public HostedEnvironmentDefinition(AssemblyName assemblyName) : this()
        {
            ApplicationName = assemblyName.Name;
        }

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