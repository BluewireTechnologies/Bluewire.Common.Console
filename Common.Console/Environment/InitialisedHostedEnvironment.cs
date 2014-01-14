using System;
using System.Collections.Generic;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Logging;

namespace Bluewire.Common.Console.Environment
{
    public class InitialisedHostedEnvironment : IExecutionEnvironment
    {
        private readonly HostedEnvironmentDefinition definition;
        private readonly IExecutionEnvironment detected;
        private readonly object instanceLock = new object();
        private List<IHostedDaemonInstance> instances = new List<IHostedDaemonInstance>();

        public InitialisedHostedEnvironment(HostedEnvironmentDefinition definition, IExecutionEnvironment detected)
        {
            this.definition = definition;
            this.detected = detected;
        }

        public void RegisterForShutdownNotification(IHostedDaemonInstance instance)
        {
            lock (instanceLock)
            {
                instances.Add(instance);
            }
        }

        private IHostedDaemonInstance[] CaptureCurrentInstances()
        {
            lock (instanceLock)
            {
                var current = instances.ToArray();
                instances = new List<IHostedDaemonInstance>();
                return current;
            }
        }

        public void RequestShutdown(TimeSpan timeout)
        {
            var victims = CaptureCurrentInstances();
            foreach (var victim in victims)
            {
                victim.RequestShutdown();
            }
            var deadline = DateTimeOffset.Now + timeout;
            foreach (var victim in victims)
            {
                victim.WaitForShutdown(deadline - DateTimeOffset.Now);
            }
        }

        public OutputDescriptorBase CreateOutputDescriptor()
        {
            var descriptor = new ServiceLogOutputDescriptor(definition.ApplicationName);
            ((IOutputDescriptorConfiguration)descriptor).SetLogRootDirectory(definition.ConsoleLogDirectory);
            return descriptor;
        }
    }
}