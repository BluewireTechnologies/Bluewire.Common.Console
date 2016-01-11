using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bluewire.Common.Console.Logging;

namespace Bluewire.Common.Console.Environment
{
    public class ApplicationEnvironment : IExecutionEnvironment
    {
        private readonly string applicationName;

        public ApplicationEnvironment() : this(ExecutionEnvironmentHelpers.GuessPrimaryAssembly())
        {
        }

        public ApplicationEnvironment(Assembly entryAssembly)
        {
            if (entryAssembly == null) throw new ArgumentNullException("entryAssembly");
            applicationName = entryAssembly.GetName().Name;
        }

        public OutputDescriptorBase CreateOutputDescriptor()
        {
            return new ConsoleOutputDescriptor(applicationName, System.Console.Error);
        }
    }
}
