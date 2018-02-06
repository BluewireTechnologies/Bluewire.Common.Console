using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Daemons
{
    public interface ITestAsConsoleApplication
    {
        int Test<T>(ApplicationEnvironment environment, IDaemonisable<T> daemon, T arguments);
    }
}
