using NUnit.Framework;

namespace Bluewire.Common.Console.NUnit3.Filesystem
{
    public static class TemporaryDirectory
    {
        public static string ForCurrentTest() => TemporaryDirectoryForTest.Allocate(TestContext.CurrentContext);
    }
}
