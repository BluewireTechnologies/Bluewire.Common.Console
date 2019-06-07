using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Bluewire.Common.Console.NUnit3.Filesystem
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class CleanTemporaryDirectoryWhenCompleteAttribute : Attribute, ITestAction
    {
        public void BeforeTest(ITest test)
        {
        }

        public void AfterTest(ITest test)
        {
            var testDetails = (TestAssembly)test;
            try
            {
                TemporaryDirectoryForTest.CleanTemporaryDirectoryForAssembly(testDetails.Assembly);
            }
            catch (IOException)
            {
                // Some libraries release files during finalisation, not disposal. This is probably
                // most common with wrappers around native libraries.
                GC.Collect();
                GC.WaitForPendingFinalizers();
                TemporaryDirectoryForTest.CleanTemporaryDirectoryForAssembly(testDetails.Assembly);
            }
        }

        public ActionTargets Targets => ActionTargets.Suite;
    }
}
