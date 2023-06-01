using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Bluewire.Common.Console.Hosting;
using Bluewire.Common.Console.UnitTests.TestHelpers;
using Bluewire.Common.ProcessJanitor;
using log4net.Config;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Hosting
{
    [TestFixture]//, Timeout(10000)]
    public class DaemonExeProcessTests
    {
        [Test]
        public void CanStartDaemonExeProcess()
        {
            var assemblyName = typeof(TestDaemon.TestDaemon).Assembly.GetName();

            var daemon = new ProcessDaemonExe(assemblyName);

            using (var process = new DaemonExeProcess(daemon))
            {
                var task = process.Run();
                Assume.That(WaitUntilDaemonStarts(task), Is.True);
            }
        }

        [TestCase("PassingValue", -1)]
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

            using (var scope = ProcessDaemonExe.FromShadowCopiedAssemblyFile(assemblyName))
            {
                var daemon = scope.Daemon.UseConfiguration(xmlDocument);

                using (var container = new DaemonExeProcess(daemon))
                {
                    var task = container.Run("--key", "Key", "--value", sentinelValue);

                    if (WaitUntilDaemonStarts(task)) container.Dispose();

                    // TestDaemon throws an unhandled exception when --value doesn't match the configured value.
                    Assert.AreEqual(expectedReturnCode, task.Result);
                }
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

                using (var scope = ProcessDaemonExe.FromShadowCopiedAssemblyFile(assemblyName))
                {
                    var daemon = scope.Daemon.UseConfiguration(xmlDocument);

                    using (var container = new DaemonExeProcess(daemon))
                    {
                        var task = container.Run();

                        WaitUntilDaemonStarts(task);

                        var configuration = daemon.ReadCurrentConfiguration();

                        var redirects = BindingRedirects.ReadFrom(configuration);
                        Assert.That(redirects.Count, Is.EqualTo(1));
                    }
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

            using (var scope = ProcessDaemonExe.FromShadowCopiedAssemblyFile(assemblyName))
            {
                var daemon = scope.Daemon
                    .UseConfiguration(xmlDocument)
                    .UseAdditionalConfiguration(additionalStream, relativePath);

                using (var container = new DaemonExeProcess(daemon))
                {
                    var task = container.Run("--key", "Key", "--value", "Value");

                    var configurationRoot = daemon.ApplicationSourceDirectory;
                    var expectedFilePath = Path.Combine(configurationRoot, relativePath);
                    Assert.That(expectedFilePath, Does.Exist);
                    Assert.That(File.ReadAllText(expectedFilePath), Is.EqualTo(content));

                    WaitUntilDaemonStarts(task);
                }
            }
        }


        [Test]
        public void CannotReuseDaemonExeProcess()
        {
            var assemblyName = typeof(TestDaemon.TestDaemon).Assembly.GetName();

            using (var scope = ProcessDaemonExe.FromShadowCopiedAssemblyFile(assemblyName))
            {
                var daemon = scope.Daemon;

                using (var container = new DaemonExeProcess(daemon))
                {
                    var task = container.Run("--key", "Key", "--value", "FailingValue");
                    task.Wait();

                    Assert.Catch<Exception>(() => container.Run());
                }
            }
        }

        [Test]
        public void CanStartDaemonExeSpecifiedByFilePathInHostedEnvironment()
        {
            var assembly = typeof(TestDaemon.TestDaemon).Assembly;

            using (var bundle = new TemporaryAssemblyBundle())
            {
                var daemonFile = bundle.Add(assembly);
                bundle.Add(typeof(DaemonRunner).Assembly);
                bundle.Add(typeof(XmlConfigurator).Assembly);

                using (var scope = ProcessDaemonExe.FromShadowCopiedAssemblyFile(daemonFile))
                {
                    var daemon = scope.Daemon;

                    using (var container = new DaemonExeProcess(daemon))
                    {
                        var task = container.Run();
                        Assert.That(WaitUntilDaemonStarts(task), Is.True);
                    }
                }
            }
        }

        [Test]
        public void HostingProcessCanCleanUpShadowCopy()
        {
            var assembly = typeof(TestDaemon.TestDaemon).Assembly;

            using (var bundle = new TemporaryAssemblyBundle())
            {
                var daemonFile = bundle.Add(assembly);
                bundle.Add(typeof(DaemonRunner).Assembly);
                bundle.Add(typeof(XmlConfigurator).Assembly);

                string temporaryContainer;
                using (var scope = ProcessDaemonExe.FromShadowCopiedAssemblyFile(daemonFile))
                {
                    var daemon = scope.Daemon;
                    temporaryContainer = scope.TemporaryContainer;

                    using (var container = new DaemonExeProcess(daemon))
                    {
                        var task = container.Run();

                        Assert.That(WaitUntilDaemonStarts(task), Is.True);
                    }
                }
                Assert.That(Directory.Exists(temporaryContainer), Is.False);
            }
        }

        [Test]
        public void JanitorProcessCanCleanUpShadowCopy()
        {
            var assembly = typeof(TestDaemon.TestDaemon).Assembly;

            using (var bundle = new TemporaryAssemblyBundle())
            {
                var daemonFile = bundle.Add(assembly);
                bundle.Add(typeof(DaemonRunner).Assembly);
                bundle.Add(typeof(XmlConfigurator).Assembly);

                using (var scope = ProcessDaemonExe.FromShadowCopiedAssemblyFile(daemonFile))
                {
                    var daemon = scope.Daemon;

                    Task janitorTask;

                    using (var container = new DaemonExeProcess(daemon))
                    {
                        var task = container.Run();

                        Assert.That(WaitUntilDaemonStarts(task), Is.True);
                        var janitor = new DaemonJanitor();
                        janitor.ErrorMessage += System.Console.WriteLine;
                        janitorTask = janitor.WatchAndTerminateOnExit(container, scope);
                        Assert.That(janitorTask.IsCompleted, Is.False);
                    }

                    Assert.That(janitorTask.Wait(5000), Is.True);
                    Assert.That(Directory.Exists(scope.TemporaryContainer), Is.False);
                }
            }
        }

        /// <summary>
        /// We have no control over how the daemon runs. We can't even be sure that a daemon will be started.
        /// Therefore, we can't wait on a 'start' event. For the purposes of testing, we do know that a daemon
        /// will start so we can simply wait a second. If the task completes beforehand though, it means the
        /// application terminated without starting a daemon.
        /// </summary>
        private static bool WaitUntilDaemonStarts(Task task)
        {
            return !task.Wait(1000);
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
