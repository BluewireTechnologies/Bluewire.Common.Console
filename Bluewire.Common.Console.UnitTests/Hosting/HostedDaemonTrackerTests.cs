using System.Linq;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Hosting;
using Bluewire.Common.Console.UnitTests.Daemons;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Hosting
{
    [TestFixture]
    public class HostedDaemonTrackerTests
    {
        private static IHostedDaemonInstance CreateDaemon(string name)
        {
            var daemonisable = new MockDaemonisable(name);
            var monitor = new HostedDaemonMonitor(daemonisable);
            monitor.Start();
            return monitor;
        }

        [Test]
        public void CanGetNamesOfRegisteredInstances()
        {
            var tracker = new HostedDaemonTracker();

            tracker.Add(CreateDaemon("A"));
            tracker.Add(CreateDaemon("B"));
            tracker.Add(CreateDaemon("C"));

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C" }, tracker.GetInfo().Select(i => i.Name));
        }

        [Test]
        [Timeout(500)]
        public void CanShutDownRegisteredInstances()
        {
            var tracker = new HostedDaemonTracker();

            tracker.Add(CreateDaemon("A"));
            tracker.Add(CreateDaemon("B"));
            tracker.Add(CreateDaemon("C"));

            tracker.Shutdown().Wait();
        }
    }
}
