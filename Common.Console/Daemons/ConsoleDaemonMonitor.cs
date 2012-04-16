using System;
using System.Threading;

namespace Bluewire.Common.Console.Daemons
{
    class ConsoleDaemonMonitor
    {
        private readonly IDaemon daemon;
        ManualResetEvent terminationEvent = new ManualResetEvent(false);
        public ConsoleDaemonMonitor(IDaemon daemon)
        {
            this.daemon = daemon;
            System.Console.CancelKeyPress += Cancel;
            System.Console.Out.WriteLine("Press CTRL-C to terminate.");
        }

        private void Cancel(object sender, ConsoleCancelEventArgs e)
        {
            System.Console.WriteLine("Shutting down.");
            e.Cancel = false;
            terminationEvent.Set();
            System.Console.CancelKeyPress -= Cancel;
        }

        public int WaitForTermination()
        {
            terminationEvent.WaitOne();
            daemon.Dispose();
            return 0;
        }
    }
}