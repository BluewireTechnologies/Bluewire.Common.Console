using System;
using System.Reflection;
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

        public ApplicationEnvironment(string name)
        {
            ApplicationName = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string ApplicationName { get; }

        public IDisposable BeginExecution()
        {
            return Disposable.Empty;
        }
    }
}
