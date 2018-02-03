using System;
using System.Linq;
using System.Threading.Tasks;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Hosting;
using Bluewire.Common.Console.Logging;

namespace Bluewire.Common.Console.Environment
{
    /// <summary>
    /// Represents a hosted environment which has been initialised by its parent for hosting daemon instances.
    /// </summary>
    /// <remarks>
    /// Tracks daemon instances and provides methods for cleanly shutting down.
    /// </remarks>
    public class InitialisedHostedEnvironment : IExecutionEnvironment
    {
        private readonly HostedEnvironmentDefinition definition;
        private readonly IExecutionEnvironment detected;
        private readonly HostedDaemonTracker instanceTracker = new HostedDaemonTracker();

        public InitialisedHostedEnvironment(HostedEnvironmentDefinition definition, IExecutionEnvironment detected)
        {
            this.definition = definition;
            this.detected = detected;
        }

        public void RegisterForShutdownNotification(IHostedDaemonInstance instance)
        {
            instanceTracker.Add(instance);
        }

        public Task RequestShutdown()
        {
            return instanceTracker.Shutdown();
        }

        public string ApplicationName => definition.ApplicationName;

        public IDisposable BeginExecution()
        {
            return new RedirectConsoleToFiles().RedirectTo(definition.ConsoleLogDirectory ?? DaemonRunnerSettings.ConsoleLogDirectory, definition.ApplicationName);
        }

        public OutputDescriptorBase CreateOutputDescriptor()
        {
            var descriptor = new ServiceLogOutputDescriptor(definition.ApplicationName);
            if (!String.IsNullOrEmpty(definition.ConsoleLogDirectory))
            {
                ((IOutputDescriptorConfiguration)descriptor).SetLogRootDirectory(definition.ConsoleLogDirectory);
            }
            return descriptor;
        }

        public IHostedDaemonInfo[] GetDaemonInfo()
        {
            return instanceTracker.GetInfo().ToArray();
        }
    }
}
