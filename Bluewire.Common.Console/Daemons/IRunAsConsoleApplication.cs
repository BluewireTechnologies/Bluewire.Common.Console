using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsConsoleApplication
    {
        int Run(ApplicationEnvironment environment, IDaemonisable daemon);
    }
}
