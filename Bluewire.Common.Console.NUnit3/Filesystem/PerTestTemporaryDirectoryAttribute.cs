using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Bluewire.Common.Console.NUnit3.Filesystem
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class PerTestTemporaryDirectoryAttribute : Attribute, ITestAction
    {
        public void BeforeTest(ITest test)
        {
        }

        public void AfterTest(ITest test)
        {
            var temporaryPath = TemporaryDirectoryForTest.Get(test.Properties);
            if (temporaryPath == null) return;
            if (!Directory.Exists(temporaryPath)) return;
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed)
            {
                try
                {
                    FileSystemHelpers.CleanDirectory(temporaryPath);
                }
                catch (IOException)
                {
                    // Some libraries release files during finalisation, not disposal. This is probably
                    // most common with wrappers around native libraries.
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    FileSystemHelpers.CleanDirectory(temporaryPath);
                }
            }
        }

        public ActionTargets Targets => ActionTargets.Test;
    }
}
