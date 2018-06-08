using Bluewire.Common.Console.Logging;
using Bluewire.Common.Console.UnitTests.TestHelpers;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Logging
{
    [TestFixture]
    public class Log4NetHelpersTests
    {
        [Test]
        public void CanCheckRunningApplicationsConfigurationFileForLog4NetSection()
        {
            Assert.That(Log4NetHelper.HasLog4NetConfiguration(), Is.False);
        }

        [Test]
        public void CanDetectLog4NetSectionInConfigurationFile()
        {
            var file = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.ApplicationConfigurationWithEmptySection.xml");
            Assert.IsTrue(Log4NetHelper.HasLog4NetConfiguration(file));
        }

        [Test]
        public void CanDetectAbsentLog4NetSectionInConfigurationFile()
        {
            var file = ConfigurationTestHelpers.GetConfigurationStreamAsTempFile("Logging.NoConfiguration.xml");
            Assert.IsFalse(Log4NetHelper.HasLog4NetConfiguration(file));
        }
    }
}
