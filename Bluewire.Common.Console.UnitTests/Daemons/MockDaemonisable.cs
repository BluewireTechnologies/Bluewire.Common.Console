using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace Bluewire.Common.Console.UnitTests.Daemons
{
    class MockDaemonisable : DaemonisableBase
    {
        public Mock<IDaemon> Daemon { get; } = new Mock<IDaemon>();

        public MockDaemonisable(string name) : base(name)
        {
        }

        public override Task<IDaemon> Start(object arguments, CancellationToken token)
        {
            return Task.FromResult(Daemon.Object);
        }
    }
}
