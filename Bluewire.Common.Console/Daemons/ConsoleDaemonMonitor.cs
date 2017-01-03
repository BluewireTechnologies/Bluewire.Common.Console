namespace Bluewire.Common.Console.Daemons
{
    class ConsoleDaemonMonitor<TArguments>
    {
        private readonly CancelMonitor cancelMonitor;
        private readonly HostedDaemonMonitor<TArguments> monitor;

        public ConsoleDaemonMonitor(IDaemonisable<TArguments> daemon)
        {
            monitor = new HostedDaemonMonitor<TArguments>(daemon);
            cancelMonitor = new CancelMonitor();
            cancelMonitor.CancelRequested += (s, e) => System.Console.Error.WriteLine("Shutting down.");
            cancelMonitor.KillRequested += (s, e) => e.Cancel = true; // Ignore kill requests.
        }

        public void Start(TArguments arguments)
        {
            monitor.Start(arguments);
        }

        public int WaitForTermination()
        {
            System.Console.Error.WriteLine("Press CTRL-C to terminate.");
            cancelMonitor.WaitForCancel();
            monitor.RequestShutdown();
            monitor.Wait();
            return 0;
        }
    }
}
