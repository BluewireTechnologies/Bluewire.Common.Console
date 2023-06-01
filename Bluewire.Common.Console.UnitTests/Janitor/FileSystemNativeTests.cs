using System.IO;
using Bluewire.Common.ProcessJanitor;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Janitor
{
    [TestFixture]
    public class FileSystemNativeTests
    {
        [Test]
        public void CanIdentifyChildOfTempDirectory()
        {
            var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            dir.Create();
            try
            {
                Assert.That(FileSystemNative.IsDescendantDirectory(new DirectoryInfo(Path.GetTempPath()), dir), Is.True);
            }
            finally
            {
                dir.Delete();
            }
        }
    }
}
