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
                FileSystemHelpers.CleanDirectory(temporaryPath);
            }
        }

        public ActionTargets Targets => ActionTargets.Test;
    }
}