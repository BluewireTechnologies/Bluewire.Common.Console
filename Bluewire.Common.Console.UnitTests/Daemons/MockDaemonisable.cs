using Moq;

namespace Bluewire.Common.Console.UnitTests.Daemons
{
    class MockDaemonisable : DaemonisableBase
    {
        public Mock<IDaemon> Daemon { get; } = new Mock<IDaemon>();

        public MockDaemonisable(string name) : base(name)
        {
        }

        public override IDaemon Start(object arguments)
        {
            return Daemon.Object;
        }
    }
}
