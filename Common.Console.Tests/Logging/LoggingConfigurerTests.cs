using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Bluewire.Common.Console.Logging;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using MbUnit.Framework;
using Moq;

namespace Bluewire.Common.Console.Tests.Logging
{
    [TestFixture]
    public class LoggingConfigurerTests
    {
        private Stream GetConfigurationStream(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = String.Format("{0}.Logging.{1}", assembly.GetName().Name, name);
            var resource = assembly.GetManifestResourceStream(resourceName);
            if (resource == null) throw new ArgumentException(String.Format("Resource {0} was not found.", resourceName), "name");
            return resource;
        }

        private TextWriter NULL_DEVICE;
        private TextWriter STDOUT;
        private TextWriter STDERR;


        [SetUp]
        public void SetUp()
        {
            NULL_DEVICE = new StringWriter();
            STDOUT = new StringWriter();
            STDERR = new StringWriter();
            LogManager.ResetConfiguration();
        }

        [TearDown]
        public void TearDown()
        {
            LogManager.ResetConfiguration();
            NULL_DEVICE.Dispose();
            STDOUT.Dispose();
            STDERR.Dispose();
        }

        [Test]
        public void ConfiguresDefaultLogging_If_EmptyConfiguration()
        {
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", NULL_DEVICE, NULL_DEVICE)))
            {
                using(var config = GetConfigurationStream("EmptyConfiguration.xml"))
                {
                    XmlConfigurator.Configure(config);
                }

                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.AreElementsEqualIgnoringOrder(new[]{
                    "Console.STDOUT",
                    "Console.STDERR",
                    "DefaultLogAppender"
                }, appenders.Select(a => a.Name));

                Assert.IsTrue(configurer.Console.IsWarnEnabled);
            }
        }

        [Test]
        public void ConfiguresDefaultLogging_If_NoConfiguration()
        {
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", NULL_DEVICE, NULL_DEVICE)))
            {
                using (var config = GetConfigurationStream("NoConfiguration.xml"))
                {
                    XmlConfigurator.Configure(config);
                }

                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.AreElementsEqualIgnoringOrder(new[]{
                    "Console.STDOUT",
                    "Console.STDERR",
                    "DefaultLogAppender"
                }, appenders.Select(a => a.Name));

                Assert.IsTrue(configurer.Console.IsWarnEnabled);
            }
        }

        [Test]
        public void ConfiguresOnlyConsoleLogging_If_ConfigurationProvidesAppendersForTheRoot()
        {
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", NULL_DEVICE, NULL_DEVICE)))
            {
                using (var config = GetConfigurationStream("ConfigureRootAppender.xml"))
                {
                    XmlConfigurator.Configure(config);
                }

                var appenders = LogManager.GetRepository().GetAppenders();

                Assert.AreElementsEqualIgnoringOrder(new[]{
                    "Console.STDOUT",
                    "Console.STDERR",
                    "ConfiguredRootAppender"
                }, appenders.Select(a => a.Name));

                Assert.IsTrue(configurer.Console.IsWarnEnabled);
            }
        }

        [Test]
        public void ConsoleErrorsGoTo_STDERR()
        {
            const string error = "Error Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDOUT, STDERR)))
            {
                using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
                {
                    XmlConfigurator.Configure(config);
                }

                configurer.Console.Error(error);
            }
            Assert.Contains(STDERR.ToString(), error);
            Assert.DoesNotContain(STDOUT.ToString(), error);
        }

        [Test]
        public void ConsoleMessagesGoTo_STDOUT()
        {
            const string message = "Info Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDOUT, STDERR)))
            {
                using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
                {
                    XmlConfigurator.Configure(config);
                }

                configurer.Console.Warn(message);
            }
            Assert.Contains(STDOUT.ToString(), message);
            Assert.DoesNotContain(STDERR.ToString(), message);
        }

        [Test]
        public void ConsoleMessagesDoNotGoTo_LogFile()
        {
            const string message = "Info Message";
            using(var defaultLog = new StringWriter())
            {
                var descriptor = new Mock<ConsoleOutputDescriptor>("Test", STDOUT, STDERR) {CallBase = true};
                descriptor.Setup(d => d.CreateDefaultLog()).Returns(new TextWriterAppender { Writer = defaultLog });
                using (var configurer = new LoggingConfigurer(descriptor.Object))
                {
                    using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
                    {
                        XmlConfigurator.Configure(config);
                    }

                    configurer.Console.Warn(message);
                }
                Assert.DoesNotContain(defaultLog.ToString(), message);
            }
        }

        [Test]
        public void RootMessagesDoNotGoTo_STDERR_Or_STDOUT()
        {
            const string message = "Info Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDOUT, STDERR)))
            {
                using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
                {
                    XmlConfigurator.Configure(config);
                }

                LogManager.GetLogger("none").Info(message);
            }
            Assert.DoesNotContain(STDOUT.ToString(), message);
            Assert.DoesNotContain(STDERR.ToString(), message);
        }

        [Test]
        public void DefaultConsoleVerbosity_OnlyShowsWarnings()
        {
            const string info = "Info Message";
            const string warning = "Warning Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDOUT, STDERR)))
            {
                using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
                {
                    XmlConfigurator.Configure(config);
                }

                configurer.Console.Info(info);
                configurer.Console.Warn(warning);
            }
            Assert.DoesNotContain(STDOUT.ToString(), info);
            Assert.Contains(STDOUT.ToString(), warning);

            Assert.DoesNotContain(STDERR.ToString(), info);
            Assert.DoesNotContain(STDERR.ToString(), warning);
        }

        [Test]
        public void SettingConsoleVerbosity_AfterInitialConfig_CanEnableInfoMessages()
        {
            const string info = "Info Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDOUT, STDERR)))
            {
                using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
                {
                    XmlConfigurator.Configure(config);
                }

                configurer.ConsoleVerbosity = Level.Info;

                configurer.Console.Info(info);
            }
            Assert.Contains(STDOUT.ToString(), info);
            Assert.DoesNotContain(STDERR.ToString(), info);
        }

        [Test]
        public void SettingConsoleVerbosity_BeforeInitialConfig_CanEnableInfoMessages()
        {
            const string info = "Info Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDOUT, STDERR)))
            {
                configurer.ConsoleVerbosity = Level.Info;

                using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
                {
                    XmlConfigurator.Configure(config);
                }

                configurer.Console.Info(info);
            }
            Assert.Contains(STDOUT.ToString(), info);
            Assert.DoesNotContain(STDERR.ToString(), info);
        }

    }
}
