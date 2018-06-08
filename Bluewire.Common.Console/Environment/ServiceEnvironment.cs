using System;
using System.Reflection;
using Bluewire.Common.Console.Daemons;

namespace Bluewire.Common.Console.Environment
{
    public class ServiceEnvironment : IExecutionEnvironment
    {
        public ServiceEnvironment() : this(ExecutionEnvironmentHelpers.GuessPrimaryAssembly())
        {
        }

        public ServiceEnvironment(Assembly entryAssembly)
        {
            if (entryAssembly == null) throw new ArgumentNullException(nameof(entryAssembly));
            ApplicationName = entryAssembly.GetName().Name;
        }

        public string ApplicationName { get; }

        public IDisposable BeginExecution()
        {
            return new RedirectConsoleToFiles().RedirectTo(DaemonRunnerSettings.GetConsoleLogDirectory(ApplicationName), ApplicationName);
        }
    }
}
