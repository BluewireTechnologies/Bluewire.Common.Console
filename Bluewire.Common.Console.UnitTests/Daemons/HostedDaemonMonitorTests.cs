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
            var daemon = new Mock<IDaemon>();
            var monitor = new HostedDaemonMonitor(daemon.Object);

            monitor.RequestShutdown();

            monitor.WaitForTermination();
        }

        [Test]
        public void RequestingShutdownCausesDaemonToBeDisposed()
        {
            var daemon = new Mock<IDaemon>();
            var monitor = new HostedDaemonMonitor(daemon.Object);

            monitor.RequestShutdown();

            monitor.WaitForTermination();

            daemon.Verify(d => d.Dispose());
        }

        [Test]
        public void CanRequestShutdownMultipleTimes()
        {
            var daemon = new Mock<IDaemon>();
            var monitor = new HostedDaemonMonitor(daemon.Object);

            monitor.RequestShutdown();
            monitor.RequestShutdown();
            monitor.RequestShutdown();

            monitor.WaitForTermination();
        }

        [Test]
        public void CanWaitForShutdown()
        {
            var daemon = new Mock<IDaemon>();
            var monitor = new HostedDaemonMonitor(daemon.Object);

            monitor.RequestShutdown();

            monitor.WaitForTermination();

            monitor.WaitForShutdown(TimeSpan.Zero);
        }

        [Test]
        public void ShutdownOccursWithoutAnExplicitWaitForTermination()
        {
            var daemon = new Mock<IDaemon>();
            var monitor = new HostedDaemonMonitor(daemon.Object);

            monitor.RequestShutdown();

            monitor.WaitForShutdown(TimeSpan.FromSeconds(5));
        }

        [Test]
        public void CanWaitForShutdownMultipleTimes()
        {
            var daemon = new Mock<IDaemon>();
            var monitor = new HostedDaemonMonitor(daemon.Object);

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
            var daemon = new Mock<IDaemon>();
            var wait = BlockShutdown(daemon);
            var monitor = new HostedDaemonMonitor(daemon.Object);

            var task = monitor.RequestShutdown();

            Assert.False(task.Wait(TimeSpan.FromMilliseconds(10)));

            wait.Set();
            monitor.WaitForTermination();

            Assert.True(task.IsCompleted);
        }

        [Test]
        public void WaitForShutdownThrowsTimeoutExceptionIfItTimesOut()
        {
            var daemon = new Mock<IDaemon>();
            var wait = BlockShutdown(daemon);
            var monitor = new HostedDaemonMonitor(daemon.Object);

            monitor.RequestShutdown();

            Assert.Throws<TimeoutException>(() => monitor.WaitForShutdown(TimeSpan.Zero));

            wait.Set();
        }

        [Test]
        public void WaitForShutdownThrowsInvalidOperationExceptionIfShutdownHasNotBeenRequested()
        {
            var daemon = new Mock<IDaemon>();
            var monitor = new HostedDaemonMonitor(daemon.Object);

            Assert.Throws<InvalidOperationException>(() => monitor.WaitForShutdown(TimeSpan.Zero));
        }

        [Test]
        public void ShutdownTaskRequestedAfterTerminationIsCompleted()
        {
            var daemon = new Mock<IDaemon>();
            var monitor = new HostedDaemonMonitor(daemon.Object);

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
