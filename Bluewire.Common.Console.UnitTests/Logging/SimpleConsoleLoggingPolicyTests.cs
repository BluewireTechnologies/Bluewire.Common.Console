using System.IO;
using System.Linq;
using Bluewire.Common.Console.Environment;
using Bluewire.Common.Console.Logging;
using Bluewire.Common.Console.UnitTests.TestHelpers;
using log4net;
using log4net.Config;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Logging
{
    [TestFixture]
    public class SimpleConsoleLoggingPolicyTests
    {
        private TextWriter NULL_DEVICE;
        private TextWriter STDERR;

        [SetUp, TearDown]
        public void ResetLoggingState()
        {
            NULL_DEVICE?.Dispose();
            STDERR?.Dispose();
            NULL_DEVICE = new StringWriter();
            STDERR = new StringWriter();
            LogManager.ResetConfiguration();
            LoggingPolicy.Reset();
        }

        [Test]
        public void ConfiguresDefaultLogging_If_EmptyConfiguration()
        {
            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.EmptyConfiguration.xml");
            XmlConfigurator.Configure(new FileInfo(configFilePath));

            var policy = new SimpleConsoleLoggingPolicy { Log4NetConfigurationFilePath = configFilePath };

            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.That(appenders.Select(a => a.Name), Is.EquivalentTo(new[]{ "Console.STDERR", "DefaultConsoleAppender" }));
                Assert.IsTrue(Log.Console.IsWarnEnabled);
            }
        }

        [Test]
        public void ConfiguresDefaultLogging_If_NoConfiguration()
        {
            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.NoConfiguration.xml");
            XmlConfigurator.Configure(new FileInfo(configFilePath));

            var policy = new SimpleConsoleLoggingPolicy { Log4NetConfigurationFilePath = configFilePath };

            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.That(appenders.Select(a => a.Name), Is.EquivalentTo(new[]{ "Console.STDERR", "DefaultConsoleAppender" }));
                Assert.IsTrue(Log.Console.IsWarnEnabled);
            }
        }

        [Test]
        public void ConfiguresDefaultLogging_If_Log4NetIsNotConfigured()
        {
            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.NoConfiguration.xml");

            var policy = new SimpleConsoleLoggingPolicy { Log4NetConfigurationFilePath = configFilePath };

            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.That(appenders.Select(a => a.Name), Is.EquivalentTo(new[]{ "Console.STDERR", "DefaultConsoleAppender" }));
                Assert.IsTrue(Log.Console.IsWarnEnabled);
            }
        }

        [Test]
        public void InitialisesLog4NetFromConfigurationFile_If_Log4NetHasNotYetBeenConfigured()
        {
            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.ConfigureRootAppender.xml");

            var policy = new SimpleConsoleLoggingPolicy { Log4NetConfigurationFilePath = configFilePath };
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.That(appenders.Select(a => a.Name), Is.EquivalentTo(new[]{ "Console.STDERR", "ConfiguredRootAppender" }));
                Assert.IsTrue(Log.Console.IsWarnEnabled);
            }
        }

        [Test]
        public void DefaultLogging_DoesNotDuplicateMessages()
        {
            const string message = "Message";

            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.NoConfiguration.xml");

            var policy = new SimpleConsoleLoggingPolicy { Log4NetConfigurationFilePath = configFilePath };

            using (new RedirectConsoleScope(NULL_DEVICE, STDERR))
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                Log.Console.Error(message);
            }
            Assert.That(STDERR.ToString(), Does.Contain(message));
            Assert.That(STDERR.ToString().IndexOf(message), Is.EqualTo(STDERR.ToString().LastIndexOf(message)));
        }

        [Test]
        public void ConfiguresOnlyConsoleLogging_If_ConfigurationProvidesAppendersForTheRoot()
        {
            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.ConfigureRootAppender.xml");
            XmlConfigurator.Configure(new FileInfo(configFilePath));

            var policy = new SimpleConsoleLoggingPolicy { Log4NetConfigurationFilePath = configFilePath };
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.That(appenders.Select(a => a.Name), Is.EquivalentTo(new[]{ "Console.STDERR", "ConfiguredRootAppender" }));
                Assert.IsTrue(Log.Console.IsWarnEnabled);
            }
        }

        [Test]
        public void ConsoleErrorsGoTo_STDERR()
        {
            const string error = "Error Message";

            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.EmptyConfiguration.xml");
            XmlConfigurator.Configure(new FileInfo(configFilePath));

            var policy = new SimpleConsoleLoggingPolicy { Log4NetConfigurationFilePath = configFilePath };
            using (new RedirectConsoleScope(NULL_DEVICE, STDERR))
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                Log.Console.Error(error);
            }
            Assert.That(STDERR.ToString(), Does.Contain(error));
        }

        [Test]
        public void ConsoleMessagesGoTo_STDERR()
        {
            const string message = "Info Message";

            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.EmptyConfiguration.xml");
            XmlConfigurator.Configure(new FileInfo(configFilePath));

            var policy = new SimpleConsoleLoggingPolicy { Log4NetConfigurationFilePath = configFilePath };

            using (new RedirectConsoleScope(NULL_DEVICE, STDERR))
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                Log.Console.Warn(message);
            }
            Assert.That(STDERR.ToString(), Does.Contain(message));
        }

        [Test]
        public void RootMessagesDoNotGoTo_STDERR()
        {
            const string message = "Info Message";

            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.ConfigureRootAppender.xml");
            XmlConfigurator.Configure(new FileInfo(configFilePath));

            var policy = new SimpleConsoleLoggingPolicy { Log4NetConfigurationFilePath = configFilePath };

            using (new RedirectConsoleScope(NULL_DEVICE, STDERR))
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                LogManager.GetLogger("none").Info(message);
            }
            Assert.That(STDERR.ToString(), Does.Not.Contain(message));
        }

        [Test]
        public void DefaultConsoleVerbosity_OnlyShowsWarnings()
        {
            const string info = "Info Message";
            const string warning = "Warning Message";

            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.EmptyConfiguration.xml");
            XmlConfigurator.Configure(new FileInfo(configFilePath));

            var policy = new SimpleConsoleLoggingPolicy { Log4NetConfigurationFilePath = configFilePath };

            using (new RedirectConsoleScope(NULL_DEVICE, STDERR))
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                Log.Console.Info(info);
                Log.Console.Warn(warning);
            }
            Assert.That(STDERR.ToString(), Does.Not.Contain(info));
            Assert.That(STDERR.ToString(), Does.Contain(warning));
        }

        [Test]
        public void SettingConsoleVerbosity_AfterInitialConfig_CanEnableInfoMessages()
        {
            const string info = "Info Message";

            var configFilePath = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.EmptyConfiguration.xml");
            // Initial config.
            XmlConfigurator.Configure(new FileInfo(configFilePath));

            var policy = new SimpleConsoleLoggingPolicy { Log4NetConfigurationFilePath = configFilePath };
            policy.Verbosity.Verbose();

            using (new RedirectConsoleScope(NULL_DEVICE, STDERR))
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                Log.Console.Info(info);
            }
            Assert.That(STDERR.ToString(), Does.Contain(info));
        }

        [Test]
        public void SettingConsoleVerbosity_WithoutInitialConfig_CanEnableInfoMessages()
        {
            const string info = "Info Message";

            var policy = new SimpleConsoleLoggingPolicy();
            policy.Verbosity.Verbose();

            using (new RedirectConsoleScope(NULL_DEVICE, STDERR))
            using (LoggingPolicy.Register(new TestEnvironment(), policy))
            {
                Log.Console.Info(info);
            }
            Assert.That(STDERR.ToString(), Does.Contain(info));
        }
    }
}
