namespace Bluewire.Common.Console.Daemons
{
    public interface IRunAsService
    {
        void Run<T>(IDaemonisable<T> daemon, string[] staticArgs);
    }
}