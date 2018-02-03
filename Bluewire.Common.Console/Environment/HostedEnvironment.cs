using System;
using Bluewire.Common.Console.Logging;

namespace Bluewire.Common.Console.Environment
{
    public class HostedEnvironment : IExecutionEnvironment
    {
        private static Exception NotInitialised()
        {
            return new InvalidOperationException("Hosted execution environment has not been initialised yet.");
        }

        public string ApplicationName
        {
            get { throw NotInitialised(); }
        }

        public IDisposable BeginExecution()
        {
            throw NotInitialised();
        }

        public OutputDescriptorBase CreateOutputDescriptor()
        {
            throw NotInitialised();
        }
    }
}
