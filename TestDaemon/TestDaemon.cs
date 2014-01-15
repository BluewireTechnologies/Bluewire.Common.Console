using Bluewire.Common.Console;

namespace TestDaemon
{
    public class TestDaemon : DaemonisableBase
    {
        public TestDaemon()
            : base("TestDaemon")
        {
        }

        public override IDaemon Start(object args)
        {
            return new Implementation();
        }

        class Implementation : IDaemon
        {
            public void Dispose()
            {
            }
        }
    }
}