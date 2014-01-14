using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bluewire.Common.Console.Logging;

namespace Bluewire.Common.Console.Environment
{
    public class HostedEnvironment : IExecutionEnvironment
    {
        private static Exception NotInitialised()
        {
            return new InvalidOperationException("Hosted execution environment has not been initialised yet.");
        }

        public OutputDescriptorBase CreateOutputDescriptor()
        {
            throw NotInitialised();
        }
    }
}
