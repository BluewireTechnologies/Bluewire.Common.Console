using System.Linq;
using System.ServiceProcess;

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

        private IDaemon instance;
        protected override void OnStart(string[] args)
        {
            var session = daemon.Configure();
            session.Parse(staticArgs.Concat(args).ToArray());
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
}