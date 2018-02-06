using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Bluewire.Common.Console.Hosting;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Hosting
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

                if (WaitUntilDaemonStarts(container, task)) container.Dispose();

                // TestDaemon throws an unhandled exception when --value doesn't match the configured value.
                Assert.AreEqual(expectedReturnCode, task.Result);
            }
        }

        [Test]
        public void AlternateConfigurationInheritsBindingRedirectsFromExistingConfiguration()
        {
            var assemblyName = typeof(TestDaemon.TestDaemon).Assembly.GetName();
            using (WriteConfigurationFile(assemblyName, @"<?xml version='1.0' encoding='utf-8' ?>
<configuration>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
      <dependentAssembly>
        <assemblyIdentity name='SomeReference' publicKeyToken='21ef50ce11b5d80f' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-1.2.3.4' newVersion='1.2.3.4' />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>"))
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(@"<configuration></configuration>");

                var daemon = new HostedDaemonExe(assemblyName).UseConfiguration(xmlDocument);

                using (var container = new DaemonExeContainer(daemon))
                {
                    var task = container.Run();

                    WaitUntilDaemonStarts(container, task);

                    var configuration = new XmlDocument();
                    configuration.Load(daemon.AppDomainSetup.ConfigurationFile);

                    var redirects = BindingRedirects.ReadFrom(configuration);
                    Assert.That(redirects.Count, Is.EqualTo(1));
                }
            }
        }

        [Test]
        public void WritesAdditionalConfigurationsRelativeToRootPath()
        {
            const string content = "Test";
            const string relativePath = "relative/path/file.txt";

            var assemblyName = typeof(TestDaemon.TestDaemon).Assembly.GetName();

            var additionalStream = new MemoryStream();
            using (var writer = new StreamWriter(additionalStream, Encoding.UTF8, 4096, true)) writer.Write(content);
            additionalStream.Position = 0;

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(@"
<configuration>
  <appSettings>
    <add key='Key' value='Value' />
  </appSettings>
</configuration>");

            var daemon = new HostedDaemonExe(assemblyName)
                .UseConfiguration(xmlDocument)
                .UseAdditionalConfiguration(additionalStream, relativePath);

            using (var container = new DaemonExeContainer(daemon))
            {
                var task = container.Run("--key", "Key", "--value", "Value");

                var configurationRoot = Path.GetDirectoryName(daemon.AppDomainSetup.ConfigurationFile);
                var expectedFilePath = Path.Combine(configurationRoot, relativePath);
                Assert.That(expectedFilePath, Does.Exist);
                Assert.That(File.ReadAllText(expectedFilePath), Is.EqualTo(content));

                WaitUntilDaemonStarts(container, task);
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

        private static IDisposable WriteConfigurationFile(AssemblyName assemblyName, string content)
        {
            var filePath = $"{new Uri(assemblyName.CodeBase).LocalPath}.config";
            File.WriteAllText(filePath, content);
            return new TemporaryConfigurationFile(filePath);
        }

        class TemporaryConfigurationFile : IDisposable
        {
            private readonly string path;

            public TemporaryConfigurationFile(string path)
            {
                this.path = path;
            }

            public void Dispose()
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
