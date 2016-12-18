using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.Console
{
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

        public abstract Task<IDaemon> Start(object arguments, CancellationToken token);

        public virtual string[] GetDependencies()
        {
            return new string[0];
        }
    }
}
