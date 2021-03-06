﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Hosting;

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
            return new RedirectConsoleToFiles().RedirectTo(definition.ConsoleLogDirectory ?? DaemonRunnerSettings.GetConsoleLogDirectory(ApplicationName), definition.ApplicationName);
        }

        public IHostedDaemonInfo[] GetDaemonInfo()
        {
            return instanceTracker.GetInfo().ToArray();
        }
    }
}
