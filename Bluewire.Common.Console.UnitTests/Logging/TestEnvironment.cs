using System;
using Bluewire.Common.Console.Environment;
using Bluewire.Common.Console.Util;

namespace Bluewire.Common.Console.UnitTests.Logging
{
    class TestEnvironment : IExecutionEnvironment
    {
        public string ApplicationName { get; set; } = "Test";
        public IDisposable BeginExecution() => Disposable.Empty;
    }
}
