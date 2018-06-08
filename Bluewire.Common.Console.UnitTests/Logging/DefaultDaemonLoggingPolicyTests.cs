using System.IO;
using System.Linq;
using Bluewire.Common.Console.Logging;
using Bluewire.Common.Console.UnitTests.TestHelpers;
using log4net;
using log4net.Config;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Logging
{
    [TestFixture]
    public class DefaultDaemonLoggingPolicyTests
    {
        [SetUp, TearDown]
        public void ResetLoggingState()
        {
            LogManager.ResetConfiguration();
            LoggingPolicy.Reset();
        }

        [Test]
        public void ConfiguresDefaultLogging_If_EmptyConfiguration()
        {
            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.EmptyConfiguration.xml");
            XmlConfigurator.Configure(new FileInfo(configFilePath));

            var policy = new DefaultDaemonLoggingPolicy() { Log4NetConfigurationFilePath = configFilePath };
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.That(appenders.Select(a => a.Name), Is.EquivalentTo(new[]{ "DefaultLogAppender" }));

                Assert.IsTrue(LogManager.GetLogger("any").IsWarnEnabled);
            }
        }

        [Test]
        public void ConfiguresDefaultLogging_If_NoConfiguration()
        {
            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.NoConfiguration.xml");
            XmlConfigurator.Configure(new FileInfo(configFilePath));

            var policy = new DefaultDaemonLoggingPolicy() { Log4NetConfigurationFilePath = configFilePath };
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.That(appenders.Select(a => a.Name), Is.EquivalentTo(new[]{ "DefaultLogAppender" }));

                Assert.IsTrue(LogManager.GetLogger("any").IsWarnEnabled);
            }
        }

        [Test]
        public void ConfiguresDefaultLogging_If_Log4NetIsNotConfigured()
        {
            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.NoConfiguration.xml");

            var policy = new DefaultDaemonLoggingPolicy() { Log4NetConfigurationFilePath = configFilePath };
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.That(appenders.Select(a => a.Name), Is.EquivalentTo(new[]{ "DefaultLogAppender" }));

                Assert.IsTrue(LogManager.GetLogger("any").IsWarnEnabled);
                Assert.IsFalse(LogManager.GetLogger("any").IsDebugEnabled);
            }
        }

        [Test]
        public void DoesNotConfigureDefaultAppender_If_ConfigurationProvidesAppendersForTheRoot()
        {
            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.ConfigureRootAppender.xml");
            XmlConfigurator.Configure(new FileInfo(configFilePath));

            var policy = new DefaultDaemonLoggingPolicy() { Log4NetConfigurationFilePath = configFilePath };
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.That(appenders.Select(a => a.Name), Is.EquivalentTo(new[]{ "ConfiguredRootAppender" }));

                Assert.IsTrue(LogManager.GetLogger("any").IsWarnEnabled);
            }
        }

        [Test]
        public void ConfigurationCanOverrideDefaultLogLevel()
        {
            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.ConfigureRootForDebug.xml");

            var policy = new DefaultDaemonLoggingPolicy() { Log4NetConfigurationFilePath = configFilePath };
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.That(appenders.Select(a => a.Name), Is.EquivalentTo(new[]{ "DefaultLogAppender" }));

                Assert.IsTrue(LogManager.GetLogger("any").IsDebugEnabled);
            }
        }
    }
}
