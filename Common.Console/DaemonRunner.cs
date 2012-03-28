using System;
using System.ServiceProcess;
using System.Threading;

namespace Bluewire.Common.Console
{
    public static class DaemonRunner
    {
        public static int Run<T>(string[] args, IDaemonisable<T> daemon)
        {
            if (NativeMethods.IsRunningAsService())
            {
                var servicesToRun = new ServiceBase[] { new DaemonService<T>(daemon) };
                ServiceBase.Run(servicesToRun);
                return 0;
            }

            return new ConsoleSession<T>(daemon.Configure()).Run(args, a => new ConsoleDaemonMonitor(daemon.Start(a)).WaitForTermination());
        }

        class DaemonService<TArguments> : ServiceBase
        {
            private readonly IDaemonisable<TArguments> daemon;

            public DaemonService(IDaemonisable<TArguments> daemon)
            {
                this.daemon = daemon;
            }

            private IDaemon instance;
            protected override void OnStart(string[] args)
            {
                var session = daemon.Configure();
                session.Parse(args);
                instance = daemon.Start(session.Arguments);
            }

            protected override void OnStop()
            {
                try
                {
                    instance.Dispose();
                }
                finally
                {
                    instance = null;
                }
            }
        }

        
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
}