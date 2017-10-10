using System;

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
            monitor.WaitForStart(cancelMonitor.GetToken());
            System.Console.Error.WriteLine("Press CTRL-C to terminate.");

            // This will terminate if the daemon shuts down voluntarily or if Ctrl-C is observed:
            monitor.Wait(cancelMonitor.GetToken());

            // Request shutdown and wait again, in case it was Ctrl-C.
            monitor.RequestShutdown();
            monitor.Wait();
            return 0;
        }
    }
}
