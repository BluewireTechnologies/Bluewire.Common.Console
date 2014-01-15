using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bluewire.Common.Console.Daemons;

namespace Bluewire.Common.Console.Hosting
{
    public class HostedDaemonTracker
    {
        private readonly object instanceLock = new object();
        private readonly List<IHostedDaemonInstance> instances = new List<IHostedDaemonInstance>();

        public void Add(IHostedDaemonInstance instance)
        {
            lock (instanceLock)
            {
                instances.Add(instance);
            }
        }

        public IEnumerable<IHostedDaemonInfo> GetInfo()
        {
            return CaptureCurrentInstances();
        }

        private void Remove(IHostedDaemonInstance instance)
        {
            // The instance must have been shut down by the time this is called.
            // Verify this:
            instance.WaitForShutdown(TimeSpan.Zero);
            lock (instanceLock)
            {
                // If already removed, this is a no-op.
                instances.Remove(instance);
            }
        }

        private IHostedDaemonInstance[] CaptureCurrentInstances()
        {
            // Threadsafe snapshot.
            lock (instanceLock)
            {
                return instances.ToArray();
            }
        }


        private Task RequestShutdown(IHostedDaemonInstance instance)
        {
            var shutdownTask = instance.RequestShutdown();
            shutdownTask.ContinueWith(_ => Remove(instance));
            return shutdownTask;
        }

        public Task Shutdown()
        {
            var victims = CaptureCurrentInstances();
            var task = new AwaitMultipleTasks();
            foreach (var victim in victims)
            {
                task.Track(RequestShutdown(victim));
            }
            return task.GetWaitTask();
        }
    }
}