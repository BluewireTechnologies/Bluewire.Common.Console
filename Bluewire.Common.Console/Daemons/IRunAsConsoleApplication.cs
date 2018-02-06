using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsConsoleApplication
    {
        int Run<T>(ApplicationEnvironment environment, IDaemonisable<T> daemon, T arguments);
    }
}
