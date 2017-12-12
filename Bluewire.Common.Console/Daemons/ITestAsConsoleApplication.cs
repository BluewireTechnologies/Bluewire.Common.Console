using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Daemons
{
    public interface ITestAsConsoleApplication
    {
        int Test(ApplicationEnvironment environment, IDaemonisable daemon);
    }
}
