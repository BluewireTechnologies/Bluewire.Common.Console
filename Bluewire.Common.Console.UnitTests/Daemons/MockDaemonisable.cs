using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace Bluewire.Common.Console.UnitTests.Daemons
{
    class MockDaemonisable : IDaemonisable
    {
        public Mock<IDaemon> Daemon { get; } = new Mock<IDaemon>();
        public string Name { get; }

        public MockDaemonisable(string name)
        {
            Name = name;
        }

        public Task<IDaemon> Start(CancellationToken token) => Task.FromResult(Daemon.Object);
        public virtual string[] GetDependencies() => new string[0];
    }
}
