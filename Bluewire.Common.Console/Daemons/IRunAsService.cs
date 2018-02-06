using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsService
    {
        int Run<T>(ServiceEnvironment environment, IDaemonisable<T> daemon, string[] staticArgs);
    }
}
