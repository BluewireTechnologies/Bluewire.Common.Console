using System;
using System.Reflection;
using System.Threading.Tasks;
using Bluewire.Common.Console.Environment;
using log4net;

namespace Bluewire.Common.Console.Hosting
{
    /// <summary>
    /// Builds an appdomain and provides methods for managing a hosted daemon within it.
    /// </summary>
    /// <remarks>
    /// This runs in the parent appdomain.
    /// </remarks>
    public class DaemonExeContainer : DaemonContainerBase
    {
        private readonly AssemblyName daemonAssemblyName;
        private readonly HostedEnvironmentDefinition definition;
        private Task<int> entryAssemblyTask;

        public DaemonExeContainer(AssemblyName daemonAssemblyName, HostedEnvironmentDefinition definition = new HostedEnvironmentDefinition()) : this(daemonAssemblyName, AppDomain.CurrentDomain.SetupInformation, definition)
        {
        }

        public DaemonExeContainer(AssemblyName daemonAssemblyName, AppDomainSetup setupInfo, HostedEnvironmentDefinition definition = new HostedEnvironmentDefinition())
            : base(daemonAssemblyName.Name, setupInfo)
        {
            this.daemonAssemblyName = daemonAssemblyName;
            this.definition = definition;
            if (String.IsNullOrEmpty(definition.ApplicationName)) definition.ApplicationName = daemonAssemblyName.Name;
        }

        /// <summary>
        /// Runs a daemon's EXE inside the container. The assembly's entry point is responsible
        /// for setting it up and parsing arguments.
        /// </summary>
        /// <remarks>
        /// The specified assembly will become the entry assembly. It cannot be shut down by the caller
        /// without shutting down the container.
        /// </remarks>
        /// <param name="args"></param>
        /// <returns></returns>
        public Task Run(params string[] args)
        {
            lock (Lock)
            {
                AssertNotDisposing();
                if (entryAssemblyTask != null) throw new InvalidOperationException("The container is already running an assembly.");
                // warning: shutdown may be initiated between now and ExecuteAssemblyByName!

                var appdomain = GetContainer();

                var invoker = (EntryPointInvoker)appdomain.CreateInstanceAndUnwrap(typeof(EntryPointInvoker).Assembly.FullName, typeof(EntryPointInvoker).FullName, false, BindingFlags.Instance | BindingFlags.Public, null, new object[] { daemonAssemblyName }, null, null);

                entryAssemblyTask = Task.Factory.StartNew(() => invoker.Invoke(args));
                entryAssemblyTask.ContinueWith(_ =>
                {
                    lock (Lock)
                    {
                        entryAssemblyTask = null;
                    }
                });
                return entryAssemblyTask;
            }
        }

        protected override void Initialise(DaemonContainerMediator mediator)
        {
            mediator.InitialiseHostedEnvironment(definition);
        }

        protected override void ShutDownContainer(DateTimeOffset deadline)
        {
            base.ShutDownContainer(deadline);
            WaitForEntryAssemblyToTerminate(deadline);
        }

        private void WaitForEntryAssemblyToTerminate(DateTimeOffset deadline)
        {
            if (entryAssemblyTask == null) return;
            if (!entryAssemblyTask.Wait(Until(deadline)))
            {
                Log.Warn("Timed out while waiting for the running assembly to terminate.");
                return;
            }
            if(entryAssemblyTask.IsCompleted) // Should always be true by now!
            {
                var exitCode = entryAssemblyTask.Result;
                if (exitCode == 0)
                {
                    Log.Debug("The running assembly terminated successfully.");
                }
                else
                {
                    Log.ErrorFormat("The running assembly terminated with exit code {0}", exitCode);
                }
            }
        }

        public static DaemonExeContainer Run(AssemblyName daemonAssemblyName, AppDomainSetup setupInfo, params string[] args)
        {
            var container = new DaemonExeContainer(daemonAssemblyName, setupInfo);
            try
            {
                container.Run(args);
                return container;
            }
            catch
            {
                container.Dispose();
                throw;
            }
        }
    }
}