using System;

namespace Bluewire.Common.Console.Environment
{
    public interface IExecutionEnvironment
    {
        string ApplicationName { get; }
        IDisposable BeginExecution();
    }
}
