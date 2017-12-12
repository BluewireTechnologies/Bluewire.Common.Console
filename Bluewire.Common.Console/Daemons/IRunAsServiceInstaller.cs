using Bluewire.Common.Console.Environment;

namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsServiceInstaller
    {
        int Run(ApplicationEnvironment environment, ServiceInstallerArguments arguments, string[] serviceArguments);
    }
}
