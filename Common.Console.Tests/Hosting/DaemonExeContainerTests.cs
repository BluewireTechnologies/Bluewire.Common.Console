using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Bluewire.Common.Console.Hosting;
using NUnit.Framework;

namespace Bluewire.Common.Console.Tests.Hosting
{
    [TestFixture, Timeout(10000)]
    public class DaemonExeContainerTests
    {
        [Test]
        public void CanStartDaemonExeInHostedEnvironment()
        {
            var assemblyName = typeof(TestDaemon.TestDaemon).Assembly.GetName();

            var daemon = new HostedDaemonExe(assemblyName);

            using (var container = new DaemonExeContainer(daemon))
            {
                var task = container.Run();
                Assume.That(WaitUntilDaemonStarts(container, task), Is.True);

                Assert.AreEqual(1, container.GetDaemonNames().Length);
            }
        }


        [Test, Ignore("Not sure how to implement this currently. We need to disable Unmanaged Code permissions in the child appdomain.")]
        public void DaemonCannotKillHostingProcess()
        {
            var assemblyName = typeof(TestDaemon.TestDaemon).Assembly.GetName();

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(@"
<configuration>
  <appSettings>
    <add key='Key' value='PassingValue' />
  </appSettings>
</configuration>");

            var daemon = new HostedDaemonExe(assemblyName).UseConfiguration(xmlDocument);

            using (var container = new DaemonExeContainer(daemon))
            {
                var task = container.Run("--environment-exit", "5");

                Assume.That(WaitUntilDaemonStarts(container, task), Is.False); // Not expecting a successful start.

                // TestDaemon throws an unhandled exception when --value doesn't match the configured value.
                Assert.AreNotEqual(0, task.Result);
            }

        }

        [TestCase("PassingValue", 0)]
        [TestCase("FailingValue", 255)]
        public void CanStartDaemonExeWithAlternateConfiguration(string sentinelValue, int expectedReturnCode)
        {
            var assemblyName = typeof(TestDaemon.TestDaemon).Assembly.GetName();

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(@"
<configuration>
  <appSettings>
    <add key='Key' value='PassingValue' />
  </appSettings>
</configuration>");

            var daemon = new HostedDaemonExe(assemblyName).UseConfiguration(xmlDocument);

            using (var container = new DaemonExeContainer(daemon))
            {
                var task = container.Run("--key", "Key", "--value", sentinelValue);

                if(WaitUntilDaemonStarts(container, task)) container.Dispose();

                // TestDaemon throws an unhandled exception when --value doesn't match the configured value.
                Assert.AreEqual(expectedReturnCode, task.Result);
            }
        }

        [Test]
        public void CannotReuseDaemonExeContainer()
        {
            var assemblyName = typeof(TestDaemon.TestDaemon).Assembly.GetName();

            var daemon = new HostedDaemonExe(assemblyName);

            using (var container = new DaemonExeContainer(daemon))
            {
                var task = container.Run("--key", "Key", "--value", "FailingValue");
                task.Wait();

                Assert.Catch<Exception>(() => container.Run());
            }
        }
        
        [Test]
        public void CanStartDaemonExeSpecifiedByFilePathInHostedEnvironment()
        {
            var assembly = typeof(TestDaemon.TestDaemon).Assembly;
            
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(path);
            try
            {
                var daemonFile = CopyAssembly(assembly, path);
                CopyAssembly(typeof(DaemonRunner).Assembly, path);

                var daemon = HostedDaemonExe.FromAssemblyFile(daemonFile);

                using (var container = new DaemonExeContainer(daemon))
                {
                    var task = container.Run();
                    Assume.That(WaitUntilDaemonStarts(container, task), Is.True);

                    Assert.AreEqual(1, container.GetDaemonNames().Length);
                }
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        private static string CopyAssembly(Assembly assembly, string directory)
        {
            var targetPath = Path.Combine(directory, Path.GetFileName(assembly.Location));
            File.Copy(assembly.Location, targetPath);
            return targetPath;
        }

        /// <summary>
        /// Invoking Main() means that we have no control over how the daemon runs. We can't even be sure
        /// that a daemon will be started. Therefore, we can't wait on a 'start' event. For the purposes of testing,
        /// we do know that a daemon will start so we can simply loop until it appears. If the task completes
        /// beforehand though, it means the application terminated without starting a daemon.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="task"></param>
        private static bool WaitUntilDaemonStarts(DaemonExeContainer container, Task task)
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
