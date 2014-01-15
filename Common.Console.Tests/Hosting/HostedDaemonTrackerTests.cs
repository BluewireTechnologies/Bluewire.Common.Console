﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Hosting;
using Moq;
using NUnit.Framework;

namespace Bluewire.Common.Console.Tests.Hosting
{
    [TestFixture]
    public class HostedDaemonTrackerTests
    {
        private IHostedDaemonInstance CreateDaemon(string name)
        {
            return new HostedDaemonMonitor(new Mock<IDaemon>().Object, name);
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
