namespace Bluewire.Common.Console
{
    public interface IDaemonisable<TArguments>
    {
        SessionArguments<TArguments> Configure();

        IDaemon Start(TArguments arguments);
    }

    public abstract class DaemonisableBase : IDaemonisable<object>
    {
        public SessionArguments<object> Configure()
        {
            return new NoArguments();
        }

        public abstract IDaemon Start(object args);
    }
}