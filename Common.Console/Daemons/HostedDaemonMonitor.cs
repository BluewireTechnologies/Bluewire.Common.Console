using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.Console.Daemons
{
    sealed class HostedDaemonMonitor : IHostedDaemonInstance
    {
        private readonly IDaemon daemon;
        private readonly string name;
        private readonly EventWaitHandle shutdownRequest = new ManualResetEvent(false);
        private bool isShuttingDown;
        private readonly TaskCompletionSource<object> shutdownTask = new TaskCompletionSource<object>();

        // EDGE CASE: What happens if the daemon is being constructed when a shutdown request comes in?
        // This monitor will not have been constructed yet, and therefore cannot be registered to receive it.

        public string Name { get { return name; } }
        public Type Type { get { return daemon.GetType(); } }


        public HostedDaemonMonitor(IDaemon daemon) : this(daemon, daemon.GetType().FullName)
        {
        }

        public HostedDaemonMonitor(IDaemon daemon, string name)
        {
            this.daemon = daemon;
            this.name = name;
        }

        private void DoAsyncShutdown()
        {
            if (isShuttingDown) return; // Already shutting down.
            lock (shutdownRequest)
            {
                if (isShuttingDown) return; // Already shutting down.
                isShuttingDown = true;
                shutdownRequest.Set();

                Terminate();
            }
        }

        private void Terminate()
        {
            daemon.Dispose();
            shutdownTask.SetResult(null);
        }

        public Task RequestShutdown()
        {
            Task.Factory.StartNew(DoAsyncShutdown);
            shutdownRequest.WaitOne();
            return shutdownTask.Task;
        }

        public void WaitForShutdown(TimeSpan timeout)
        {
            if (!isShuttingDown) throw new InvalidOperationException("Shutdown has not been requested.");
            if(!shutdownTask.Task.Wait(timeout)) throw new TimeoutException();
        }

        public void WaitForTermination()
        {
            shutdownTask.Task.Wait();
        }
    }
}
