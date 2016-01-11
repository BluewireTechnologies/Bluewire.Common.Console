using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bluewire.Common.Console.Logging;

namespace Bluewire.Common.Console.Environment
{
    public class ServiceEnvironment : IExecutionEnvironment
    {
        private readonly string applicationName;

        public ServiceEnvironment() : this(ExecutionEnvironmentHelpers.GuessPrimaryAssembly())
        {
        }

        public ServiceEnvironment(Assembly entryAssembly)
        {
            if (entryAssembly == null) throw new ArgumentNullException("entryAssembly");
            applicationName = entryAssembly.GetName().Name;
        }

        public OutputDescriptorBase CreateOutputDescriptor()
        {
            return new ServiceLogOutputDescriptor(applicationName);
        }
    }
}
