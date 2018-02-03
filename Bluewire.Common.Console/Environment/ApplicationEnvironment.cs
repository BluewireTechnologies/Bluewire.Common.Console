using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bluewire.Common.Console.Logging;
using Bluewire.Common.Console.Util;

namespace Bluewire.Common.Console.Environment
{
    public class ApplicationEnvironment : IExecutionEnvironment
    {
        public ApplicationEnvironment() : this(ExecutionEnvironmentHelpers.GuessPrimaryAssembly())
        {
        }

        public ApplicationEnvironment(Assembly entryAssembly)
        {
            if (entryAssembly == null) throw new ArgumentNullException("entryAssembly");
            ApplicationName = entryAssembly.GetName().Name;
        }

        public string ApplicationName { get; }

        public IDisposable BeginExecution()
        {
            return Disposable.Empty;
        }

        public OutputDescriptorBase CreateOutputDescriptor()
        {
            return new ConsoleOutputDescriptor(ApplicationName, System.Console.Error);
        }
    }
}
