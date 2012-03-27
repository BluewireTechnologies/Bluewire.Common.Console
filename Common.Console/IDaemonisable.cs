namespace Bluewire.Common.Console
{
    public interface IDaemonisable<TArguments>
    {
        SessionArguments<TArguments> Configure();

        IDaemon Start(TArguments arguments);
    }
}