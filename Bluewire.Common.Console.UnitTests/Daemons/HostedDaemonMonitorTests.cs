using System;
using System.Threading;
using Bluewire.Common.Console.Daemons;
using Moq;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Daemons
{
    [TestFixture]
    public class HostedDaemonMonitorTests
    {
        [Test]
        [Timeout(500)]
        public void RequestingShutdownCausesMonitorToTerminate()
        {
            var daemonisable = new MockDaemonisable("MockDaemon");
            var monitor = new HostedDaemonMonitor<object>(daemonisable);
            monitor.Start(new object());

            monitor.RequestShutdown();

            monitor.WaitForTermination();
        }

        [Test]
        public void RequestingShutdownCausesDaemonToBeDisposed()
        {
            var daemonisable = new MockDaemonisable("MockDaemon");
            var monitor = new HostedDaemonMonitor<object>(daemonisable);
            monitor.Start(new object());

            monitor.RequestShutdown();

            monitor.WaitForTermination();

            daemonisable.Daemon.Verify(d => d.Dispose());
        }

        [Test]
        public void CanRequestShutdownMultipleTimes()
        {
            var daemonisable = new MockDaemonisable("MockDaemon");
            var monitor = new HostedDaemonMonitor<object>(daemonisable);
            monitor.Start(new object());

            monitor.RequestShutdown();
            monitor.RequestShutdown();
            monitor.RequestShutdown();

            monitor.WaitForTermination();
        }

        [Test]
        public void CanWaitForShutdown()
        {
            var daemonisable = new MockDaemonisable("MockDaemon");
            var monitor = new HostedDaemonMonitor<object>(daemonisable);
            monitor.Start(new object());

            monitor.RequestShutdown();

            monitor.WaitForTermination();

            monitor.WaitForShutdown(TimeSpan.Zero);
        }

        [Test]
        public void ShutdownOccursWithoutAnExplicitWaitForTermination()
        {
            var daemonisable = new MockDaemonisable("MockDaemon");
            var monitor = new HostedDaemonMonitor<object>(daemonisable);
            monitor.Start(new object());

            monitor.RequestShutdown();

            monitor.WaitForShutdown(TimeSpan.FromSeconds(5));
        }

        [Test]
        public void CanWaitForShutdownMultipleTimes()
        {
            var daemonisable = new MockDaemonisable("MockDaemon");
            var monitor = new HostedDaemonMonitor<object>(daemonisable);
            monitor.Start(new object());

            monitor.RequestShutdown();

            monitor.WaitForTermination();

            monitor.WaitForShutdown(TimeSpan.Zero);
            monitor.WaitForShutdown(TimeSpan.Zero);
            monitor.WaitForShutdown(TimeSpan.Zero);
            monitor.WaitForShutdown(TimeSpan.Zero);
        }

        [Test]
        public void ShutdownTaskCompletesWhenTerminationCompletes()
        {
            var daemonisable = new MockDaemonisable("MockDaemon");
            var wait = BlockShutdown(daemonisable.Daemon);
            var monitor = new HostedDaemonMonitor<object>(daemonisable);
            monitor.Start(new object());

            var task = monitor.RequestShutdown();

            Assert.False(task.Wait(TimeSpan.FromMilliseconds(10)));

            wait.Set();
            monitor.WaitForTermination();

            Assert.True(task.IsCompleted);
        }

        [Test]
        public void WaitForShutdownThrowsTimeoutExceptionIfItTimesOut()
        {
            var daemonisable = new MockDaemonisable("MockDaemon");
            var wait = BlockShutdown(daemonisable.Daemon);
            var monitor = new HostedDaemonMonitor<object>(daemonisable);
            monitor.Start(new object());

            monitor.RequestShutdown();

            Assert.Throws<TimeoutException>(() => monitor.WaitForShutdown(TimeSpan.Zero));

            wait.Set();
        }

        [Test]
        public void WaitForShutdownThrowsInvalidOperationExceptionIfShutdownHasNotBeenRequested()
        {
            var daemonisable = new MockDaemonisable("MockDaemon");
            var monitor = new HostedDaemonMonitor<object>(daemonisable);
            monitor.Start(new object());

            Assert.Throws<InvalidOperationException>(() => monitor.WaitForShutdown(TimeSpan.Zero));
        }

        [Test]
        public void ShutdownTaskRequestedAfterTerminationIsCompleted()
        {
            var daemonisable = new MockDaemonisable("MockDaemon");
            var monitor = new HostedDaemonMonitor<object>(daemonisable);
            monitor.Start(new object());

            monitor.RequestShutdown();

            monitor.WaitForTermination();

            var task = monitor.RequestShutdown();
            Assert.True(task.IsCompleted);
        }

        private EventWaitHandle BlockShutdown(Mock<IDaemon> daemon)
        {
            var handle = new ManualResetEvent(false);
            daemon.Setup(d => d.Dispose()).Callback(() => handle.WaitOne(TimeSpan.FromSeconds(1)));
            return handle;
        }
    }
}
