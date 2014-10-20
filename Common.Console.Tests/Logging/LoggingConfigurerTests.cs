using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Bluewire.Common.Console.Logging;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using NUnit.Framework;
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

        private Configuration OpenConfigurationFile(string filename)
        {
            return ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = filename }, ConfigurationUserLevel.None);
        }

        private string GetConfigurationStreamAsTempFile(string name)
        {
            var file = Path.GetTempFileName();
            using (var configStream = GetConfigurationStream(name))
            {
                using (var reader = new StreamReader(configStream))
                {
                    File.WriteAllText(file, reader.ReadToEnd());
                }
                return file;
            }
        }

        private TextWriter NULL_DEVICE;
        private TextWriter STDERR;


        [SetUp]
        public void SetUp()
        {
            NULL_DEVICE = new StringWriter();
            STDERR = new StringWriter();
            LogManager.ResetConfiguration();
        }

        [TearDown]
        public void TearDown()
        {
            LogManager.ResetConfiguration();
            NULL_DEVICE.Dispose();
            STDERR.Dispose();
        }

        [Test]
        public void ConfiguresDefaultLogging_If_EmptyConfiguration()
        {
            using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
            {
                XmlConfigurator.Configure(config);
            }
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", NULL_DEVICE)))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                AssertDefaultAppenders(appenders);

                Assert.IsTrue(configurer.Console.IsWarnEnabled);
            }
        }

        [Test]
        public void ConfiguresDefaultLogging_If_NoConfiguration()
        {
            using (var config = GetConfigurationStream("NoConfiguration.xml"))
            {
                XmlConfigurator.Configure(config);
            }
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", NULL_DEVICE)))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                AssertDefaultAppenders(appenders);

                Assert.IsTrue(configurer.Console.IsWarnEnabled);
            }
        }
        [Test]
        public void ConfiguresDefaultLogging_If_Log4NetIsNotConfigured()
        {
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", NULL_DEVICE)))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                AssertDefaultAppenders(appenders);

                Assert.IsTrue(configurer.Console.IsWarnEnabled);
            }
        }

        private void AssertDefaultAppenders(IAppender[] appenders)
        {
            CollectionAssert.AreEquivalent(new[]{
                "Console.STDERR",
                "DefaultLogAppender"
            }, appenders.Select(a => a.Name));
        }

        [Test]
        public void ConfiguresOnlyConsoleLogging_If_ConfigurationProvidesAppendersForTheRoot()
        {
            using (var config = GetConfigurationStream("ConfigureRootAppender.xml"))
            {
                XmlConfigurator.Configure(config);
            }
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", NULL_DEVICE)))
            {
                var appenders = LogManager.GetRepository().GetAppenders();

                CollectionAssert.AreEquivalent(new[]{
                    "Console.STDERR",
                    "ConfiguredRootAppender"
                }, appenders.Select(a => a.Name));

                Assert.IsTrue(configurer.Console.IsWarnEnabled);
            }
        }

        [Test]
        public void ConsoleErrorsGoTo_STDERR()
        {
            using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
            {
                XmlConfigurator.Configure(config);
            }
            const string error = "Error Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDERR)))
            {
                configurer.Console.Error(error);
            }
            Assert.That(STDERR.ToString(), Is.StringContaining(error));
        }

        [Test]
        public void ConsoleMessagesGoTo_STDERR()
        {
            using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
            {
                XmlConfigurator.Configure(config);
            }
            const string message = "Info Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDERR)))
            {
                configurer.Console.Warn(message);
            }
            Assert.That(STDERR.ToString(), Is.StringContaining(message));
        }

        [Test]
        public void ConsoleMessagesDoNotGoTo_LogFile()
        {
            using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
            {
                XmlConfigurator.Configure(config);
            }

            const string message = "Info Message";
            using(var defaultLog = new StringWriter())
            {
                var descriptor = new Mock<ConsoleOutputDescriptor>("Test", STDERR) {CallBase = true};
                descriptor.Setup(d => d.CreateDefaultLog()).Returns(new TextWriterAppender { Writer = defaultLog });
                using (var configurer = new LoggingConfigurer(descriptor.Object))
                {
                    configurer.Console.Warn(message);
                }
                Assert.That(defaultLog.ToString(),  Is.Not.StringContaining(message));
            }
        }

        [Test]
        public void RootMessagesDoNotGoTo_STDERR()
        {
            using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
            {
                XmlConfigurator.Configure(config);
            }
            const string message = "Info Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDERR)))
            {
                LogManager.GetLogger("none").Info(message);
            }
            Assert.That(STDERR.ToString(), Is.Not.StringContaining(message));
        }

        [Test]
        public void DefaultConsoleVerbosity_OnlyShowsWarnings()
        {
            using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
            {
                XmlConfigurator.Configure(config);
            }

            const string info = "Info Message";
            const string warning = "Warning Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDERR)))
            {
                configurer.Console.Info(info);
                configurer.Console.Warn(warning);
            }
            Assert.That(STDERR.ToString(), Is.Not.StringContaining(info));
            Assert.That(STDERR.ToString(), Is.StringContaining(warning));
        }

        [Test]
        public void SettingConsoleVerbosity_AfterInitialConfig_CanEnableInfoMessages()
        {
            using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
            {
                XmlConfigurator.Configure(config);
            }
            const string info = "Info Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDERR)))
            {
                configurer.ConsoleVerbosity = Level.Info;

                configurer.Console.Info(info);
            }
            Assert.That(STDERR.ToString(), Is.StringContaining(info));
        }

        [Test]
        public void SettingConsoleVerbosity_BeforeInitialConfig_CanEnableInfoMessages()
        {
            using (var config = GetConfigurationStream("EmptyConfiguration.xml"))
            {
                XmlConfigurator.Configure(config);
            }

            const string info = "Info Message";
            using (var configurer = new LoggingConfigurer(new ConsoleOutputDescriptor("Test", STDERR)))
            {
                configurer.ConsoleVerbosity = Level.Info;   

                configurer.Console.Info(info);
            }
            Assert.That(STDERR.ToString(), Is.StringContaining(info));
        }

        [Test]
        public void CanGetRunningApplicationsConfigurationFile()
        {
            var configuration = Log.GetApplicationConfiguration();
            Assert.AreEqual("TestValue", configuration.AppSettings.Settings["TestKey"].Value);
        }

        [Test]
        public void CanDetectLog4NetSectionInConfigurationFile()
        {
            var file = GetConfigurationStreamAsTempFile("ApplicationConfigurationWithEmptySection.xml");
            var configuration = OpenConfigurationFile(file);

            Assert.IsTrue(Log.HasLog4NetConfiguration(configuration));

            File.Delete(file);
        }

        [Test]
        public void CanDetectAbsentLog4NetSectionInConfigurationFile()
        {
            var file = GetConfigurationStreamAsTempFile("NoConfiguration.xml");
            var configuration = OpenConfigurationFile(file);

            Assert.IsFalse(Log.HasLog4NetConfiguration(configuration));

            File.Delete(file);
        }
    }
}
