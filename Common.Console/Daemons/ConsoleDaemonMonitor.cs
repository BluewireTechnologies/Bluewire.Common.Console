namespace Bluewire.Common.Console.Daemons
{
    class ConsoleDaemonMonitor
    {
        private readonly IDaemon daemon;
        
        private CancelMonitor cancelMonitor;

        public ConsoleDaemonMonitor(IDaemon daemon)
        {
            this.daemon = daemon;
            cancelMonitor = new CancelMonitor();
            cancelMonitor.CancelRequested += (s, e) => System.Console.Error.WriteLine("Shutting down.");
            cancelMonitor.KillRequested += (s, e) => e.Cancel = true; // Ignore kill requests.

            System.Console.Error.WriteLine("Press CTRL-C to terminate.");
        }

        public int WaitForTermination()
        {
            cancelMonitor.WaitForCancel();
            daemon.Dispose();
            return 0;
        }
    }
}