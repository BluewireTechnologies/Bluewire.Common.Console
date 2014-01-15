using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsHostedService
    {
        void Run<T>(InitialisedHostedEnvironment environment, IDaemonisable<T> daemon, T arguments);
    }
}