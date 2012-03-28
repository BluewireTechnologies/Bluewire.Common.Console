using System;

namespace Bluewire.Common.Console
{
    public interface IDaemon : IDisposable
    {
    }

    public interface IStartableStoppable
    {
        void Start();
        void Stop();
    }

    public class StartableStoppableDaemonSession : IDaemon
    {
        private readonly IStartableStoppable instance;

        public StartableStoppableDaemonSession(IStartableStoppable instance)
        {
            this.instance = instance;
            instance.Start();
        }

        public void Dispose()
        {
            instance.Stop();
        }
    }
}