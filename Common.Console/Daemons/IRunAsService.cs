using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsService
    {
        void Run<T>(ServiceEnvironment environment, IDaemonisable<T> daemon, string[] staticArgs);
    }
}