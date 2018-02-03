using System;
using Bluewire.Common.Console.Logging;

namespace Bluewire.Common.Console.Environment
{
    public interface IExecutionEnvironment
    {
        string ApplicationName { get; }
        IDisposable BeginExecution();
        OutputDescriptorBase CreateOutputDescriptor();
    }
}
