using System;
using System.Diagnostics;
using System.Security.Permissions;
using System.Security.Policy;
using log4net;

namespace Bluewire.Common.Console.Hosting
{
    /// <summary>
    /// Builds an appdomain and provides methods for managing a hosted daemon within it.
    /// </summary>
    /// <remarks>
    /// This runs in the parent appdomain.
    /// </remarks>
    public abstract class DaemonContainerBase : IDisposable
    {
        protected IHostingBehaviour Behaviour { get; private set; }
        protected readonly ILog Log;

        private readonly string containerName;

        protected readonly object Lock = new object();
        private bool disposing;
        private AppDomain container;
        private DaemonContainerMediator mediator;

        protected DaemonContainerBase(string containerName) : this(containerName, new DefaultHostingBehaviour(AppDomain.CurrentDomain.SetupInformation))
        {
        }

        protected DaemonContainerBase(string containerName, IHostingBehaviour behaviour)
        {
            Behaviour = behaviour;
            Log = LogManager.GetLogger(containerName + ".Container");
            this.containerName = containerName;
            ShutdownTimeout = TimeSpan.FromSeconds(15);
        }

        public TimeSpan ShutdownTimeout { get; set; }

        public string[] GetDaemonNames()
        {
            lock (Lock)
            {
                if (mediator == null) return new string[0];
                return mediator.GetDaemonNames();
            }
        }

        protected abstract void Initialise(DaemonContainerMediator mediator);

        /// <summary>
        /// Called before the appdomain is unloaded.
        /// Should clean up anything running in it.
        /// </summary>
        /// <remarks>
        /// Base implementation asks the Mediator to shut down tracked daemons.
        /// </remarks>
        /// <param name="deadline"></param>
        protected virtual void ShutDownContainer(DateTimeOffset deadline)
        {
            ShutDownDaemons(deadline);
        }

        public void Dispose()
        {
            lock (Lock)
            {
                disposing = true;
                var deadline = DateTimeOffset.Now + ShutdownTimeout;
                ShutDownContainer(deadline);
                if (container != null)
                {
                    mediator = null;
                    AppDomain.Unload(container);
                    Behaviour.OnAfterStop();
                    container = null;
                }
            }
        }

        protected static TimeSpan Until(DateTimeOffset deadline)
        {
            var duration = deadline - DateTimeOffset.Now;
            if (duration <= TimeSpan.Zero) return TimeSpan.Zero;
            return duration;
        }

        protected DaemonContainerMediator GetMediator()
        {
            return mediator;
        }

        protected AppDomain GetContainer()
        {
            lock (Lock)
            {
                AssertNotDisposing();
                if (container == null)
                {
                    CreateContainer();
                }
                return container;
            }
        }

        protected void ShutDownDaemons(DateTimeOffset deadline)
        {
            if (mediator == null) return;
            if (!mediator.RequestShutdown(Until(deadline)))
            {
                Log.Warn("Timed out while waiting for daemons to shut down.");
            }
        }

        // Must only be called from inside the lock.
        protected void AssertNotDisposing()
        {
            if (disposing) throw new ObjectDisposedException("DaemonContainer has been disposed.");
        }

        private void CreateContainer()
        {
            // Must have the lock!

            AssertNotDisposing();
            Debug.Assert(container == null);
            Debug.Assert(mediator == null);

            Behaviour.OnBeforeStart();

            var appDomainSetup = Behaviour.CreateAppDomainSetup();
            var containerInstance = AppDomain.CreateDomain(containerName, null, appDomainSetup);
            try
            {
                var mediatorType = typeof(DaemonContainerMediator);
                var mediatorInstance = (DaemonContainerMediator)containerInstance.CreateInstanceAndUnwrap(mediatorType.Assembly.FullName, mediatorType.FullName);

                Initialise(mediatorInstance);

                mediator = mediatorInstance;
                container = containerInstance;
            }
            catch
            {
                AppDomain.Unload(containerInstance);
                Behaviour.OnAfterStop();
                container = null;
                throw;
            }

            Debug.Assert(container != null);
            Debug.Assert(mediator != null);
        }

    }
}
