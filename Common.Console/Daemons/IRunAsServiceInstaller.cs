namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsServiceInstaller
    {
        int Run<T>(IDaemonisable<T> daemon, ServiceInstallerArguments arguments, string[] serviceArguments);
    }
}