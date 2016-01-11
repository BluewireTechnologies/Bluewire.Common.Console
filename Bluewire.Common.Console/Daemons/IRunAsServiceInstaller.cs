using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsServiceInstaller
    {
        int Run<T>(ApplicationEnvironment environment, ServiceInstallerArguments<T> arguments, string[] serviceArguments);
    }
}