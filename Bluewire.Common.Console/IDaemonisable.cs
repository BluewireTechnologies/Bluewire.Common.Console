using System;

namespace Bluewire.Common.Console
{
    public interface IDaemonisable<TArguments>
    {
        string Name { get; }

        SessionArguments<TArguments> Configure();

        IDaemon Start(TArguments arguments);
        string[] GetDependencies();
    }

    public abstract class DaemonisableBase : IDaemonisable<object>
    {
        public string Name { get; private set; }
        protected DaemonisableBase(string name)
        {
            Name = name;
        }

        public SessionArguments<object> Configure()
        {
            return new NoArguments();
        }

        public abstract IDaemon Start(object args);

        public virtual string[] GetDependencies()
        {
            return new string[0];
        }
    }
}