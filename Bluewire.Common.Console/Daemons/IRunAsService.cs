using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsService
    {
        int Run(ServiceEnvironment environment, IDaemonisable daemon, string[] staticArgs);
    }
}
