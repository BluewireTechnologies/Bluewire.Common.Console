using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsHostedService
    {
        int Run(InitialisedHostedEnvironment environment, IDaemonisable daemon);
    }
}
