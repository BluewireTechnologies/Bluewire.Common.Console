using System;
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
            TemporaryDirectoryForTest.CleanTemporaryDirectoryForAssembly(testDetails.Assembly);
        }

        public ActionTargets Targets => ActionTargets.Suite;
    }
}
