﻿using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests
{
    [TestFixture]
    public class CancelMonitorTests
    {
        private static ConsoleCancelEventArgs CreateEventArgs(ConsoleSpecialKey key = ConsoleSpecialKey.ControlC)
        {
            var constructor = typeof(ConsoleCancelEventArgs).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
            return (ConsoleCancelEventArgs)constructor.Invoke(new object[] { key });
        }

        [Test]
        public void FirstCancelRequestTriggersCancellation()
        {
            var monitor = new CancelMonitor();
            var eventArgs = CreateEventArgs();
            monitor.RequestCancel(new object(), eventArgs);

            Assert.IsTrue(monitor.GetToken().IsCancellationRequested);
        }

        [Test]
        public void FirstCancelRequestDoesNotKillApplication()
        {
            var monitor = new CancelMonitor();
            var eventArgs = CreateEventArgs();
            monitor.RequestCancel(new object(), eventArgs);

            Assert.IsTrue(eventArgs.Cancel);
        }

        [Test]
        public void SecondCancelRequestTriggersKill()
        {
            var monitor = new CancelMonitor();

            monitor.RequestCancel(new object(), CreateEventArgs());

            var eventArgs = CreateEventArgs();
            monitor.RequestCancel(new object(), CreateEventArgs());

            Assert.IsFalse(eventArgs.Cancel);
        }
    }
}
