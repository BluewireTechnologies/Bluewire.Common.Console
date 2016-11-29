using Bluewire.Common.Console.NUnit3.Filesystem;
using NUnit.Framework;

namespace Bluewire.Common.Console.NUnit3.UnitTests.Filesystem
{
    [TestFixture]
    public class TemporaryDirectoryTests
    {
        [Test]
        public void CanAcquireTemporaryDirectoryForTestWithVeryVeryVeryVeryVeryVeryVeryVeryLongName()
        {
            var tempDir = TemporaryDirectory.ForCurrentTest();
            Assert.That(tempDir.Length, Is.LessThan(260)); // Absolute limit.
            Assert.That(tempDir.Length, Is.LessThan(200)); // Sensible margin of error.

        }
    }
}
