using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bluewire.Common.Console.Daemons
{
    sealed class HostedDaemonMonitor : IHostedDaemonInstance
    {
        private readonly IDaemon daemon;
        private readonly EventWaitHandle shutdownRequest = new ManualResetEvent(false);
        private bool isShuttingDown;
        private readonly EventWaitHandle shutdownComplete = new ManualResetEvent(false);

        // EDGE CASE: What happens if the daemon is being constructed when a shutdown request comes in?
        // This monitor will not have been constructed yet, and therefore cannot be registered to receive it.


        public HostedDaemonMonitor(IDaemon daemon)
        {
            this.daemon = daemon;
        }

        public void RequestShutdown()
        {
            shutdownRequest.Set();
            isShuttingDown = true;
        }

        public void WaitForShutdown(TimeSpan timeout)
        {
            if (!isShuttingDown) throw new InvalidOperationException("Shutdown has not been requested.");
            if (!shutdownComplete.WaitOne(timeout)) throw new TimeoutException();
        }

        public int WaitForTermination()
        {
            shutdownRequest.WaitOne();
            daemon.Dispose();
            shutdownComplete.Set();
            return 0;
        }
    }
}
