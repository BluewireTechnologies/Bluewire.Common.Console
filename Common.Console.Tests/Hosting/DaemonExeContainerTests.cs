using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bluewire.Common.Console.Hosting;
using NUnit.Framework;

namespace Bluewire.Common.Console.Tests.Hosting
{
    [TestFixture]
    public class DaemonExeContainerTests
    {
        [Test]
        [Timeout(10000)]
        public void CanStartDaemonExeInHostedEnvironment()
        {
            var assemblyName = typeof(TestDaemon.TestDaemon).Assembly.GetName();

            using(var container = new DaemonExeContainer(assemblyName, AppDomain.CurrentDomain.SetupInformation))
            {
                var task = container.Run();
                Assert.True(WaitUntilDaemonStarts(container, task));

                Assert.AreEqual(1, container.GetDaemonNames().Length);
            }
        }

        /// <summary>
        /// Invoking Main() means that we have no control over how the daemon runs. We can't even be sure
        /// that a daemon will be started. Therefore, we can't wait on a 'start' event. For the purposes of testing,
        /// we do know that a daemon will start so we can simply loop until it appears. If the task completes
        /// beforehand though, it means the application terminated without starting a daemon.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="task"></param>
        private bool WaitUntilDaemonStarts(DaemonExeContainer container, Task task)
        {
            while (!task.Wait(10))
            {
                if (container.GetDaemonNames().Any()) return true;
            }
            // Task completed, so the app must've terminated.
            return false;
        }
    }
}
