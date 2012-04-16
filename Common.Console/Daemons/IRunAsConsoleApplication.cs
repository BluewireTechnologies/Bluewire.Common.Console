namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsConsoleApplication
    {
        int Run<T>(IDaemonisable<T> daemon, T arguments);
    }
}