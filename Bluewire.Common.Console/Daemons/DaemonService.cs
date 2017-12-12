using System.Linq;
using System.ServiceProcess;
using Bluewire.Common.Console.Arguments;

namespace Bluewire.Common.Console.Daemons
{
    class DaemonService : ServiceBase
    {
        private readonly IDaemonisable daemon;
        private readonly string[] staticArgs;

        public DaemonService(IDaemonisable daemon, string[] staticArgs)
        {
            this.daemon = daemon;
            this.staticArgs = staticArgs;
        }

        private HostedDaemonMonitor instance;
        protected override void OnStart(string[] args)
        {
            OnStop();
            var session = new SessionArguments();
            session.Options.AddCollector(daemon as IReceiveOptions);
            session.ArgumentList.AddCollector(daemon as IReceiveArgumentList);
            session.Parse(staticArgs.Concat(args).ToArray());

            instance = new HostedDaemonMonitor(daemon);
            instance.Start();
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
