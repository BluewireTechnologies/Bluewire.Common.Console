namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsServiceInstaller
    {
        int Run<T>(ServiceInstallerArguments<T> arguments, string[] serviceArguments);
    }
}