using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.Console.Daemons
{
    class DaemonService<TArguments> : ServiceBase
    {
        private readonly IDaemonisable<TArguments> daemon;
        private readonly string[] staticArgs;

        public DaemonService(IDaemonisable<TArguments> daemon, string[] staticArgs)
        {
            this.daemon = daemon;
            this.staticArgs = staticArgs;
        }

        private HostedDaemonMonitor<TArguments> instance;
        protected override void OnStart(string[] args)
        {
            OnStop();
            var session = daemon.Configure();
            session.Parse(staticArgs.Concat(args).ToArray());

            instance = new HostedDaemonMonitor<TArguments>(daemon);
            instance.Start(session.Arguments);
            instance.WaitForStart();
        }

        protected override void OnStop()
        {
            try
            {
                instance?.RequestShutdown()?.Wait();
            }
            finally
            {
                instance = null;
            }
        }
    }
}
