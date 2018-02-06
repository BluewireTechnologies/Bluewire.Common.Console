using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.Console.Daemons
{
    sealed class HostedDaemonMonitor<TArguments> : IHostedDaemonInstance
    {
        private readonly CancellationTokenSource shutdownToken = new CancellationTokenSource();
        private readonly IDaemonisable<TArguments> daemon;
        private Task CompletedTask
        {
            get
            {
                var tcs = new TaskCompletionSource<object>();
                tcs.SetResult(null);
                return tcs.Task;
            }
        }

        /// <summary>
        /// Represents initialisation of the daemon instance.
        /// </summary>
        private Task<IDaemon> createDaemonTask;
        /// <summary>
        /// Represents entire lifetime of the daemon instance.
        /// </summary>
        private Task lifetimeTask;
        /// <summary>
        /// Represents shutdown of the daemon instance. Initialised on first
        /// request after shutdown has begun.
        /// </summary>
        private Task shutdownTask;

        public string Name => daemon.Name;
        public bool ShutdownRequested => shutdownToken.IsCancellationRequested;

        public HostedDaemonMonitor(IDaemonisable<TArguments> daemon)
        {
            this.daemon = daemon;
        }

        public Task Start(TArguments arguments)
        {
            if (createDaemonTask != null) throw new InvalidOperationException("Instance already started.");
            lock (shutdownToken)
            {
                if (this.createDaemonTask != null) throw new InvalidOperationException("Instance already started.");
                createDaemonTask = Task.Run(() => daemon.Start(arguments, shutdownToken.Token));
                lifetimeTask = Task.Run(async () => { using (await createDaemonTask) await shutdownToken.Token.WaitHandle.AsTask(); });
                return lifetimeTask;
            }
        }

        private Task GetShutdownCompletionTask()
        {
            lock (shutdownToken)
            {
                if (lifetimeTask == null) return CompletedTask;
                if (shutdownTask == null) shutdownTask = AwaitShutdownCompletion();
                return shutdownTask;
            }
        }

        private async Task AwaitShutdownCompletion()
        {
            await createDaemonTask.ConfigureAwait(false);
            await lifetimeTask.GetCompletionOnlyTask().ConfigureAwait(false);
        }

        /// <summary>
        /// Triggers shutdown of the daemon, or cancels initialisation.
        /// </summary>
        /// <returns></returns>
        public Task RequestShutdown()
        {
            shutdownToken.Cancel();
            return GetShutdownCompletionTask();
        }

        /// <summary>
        /// Wait for startup, propagating errors.
        /// </summary>
        public void WaitForStart(CancellationToken token = default(CancellationToken))
        {
            createDaemonTask.WaitWithUnwrapExceptions(token);
        }

        /// <summary>
        /// Wait on total shutdown of the daemon for a limited period of time.
        /// Propagates startup failures but not shutdown failures.
        /// </summary>
        /// <param name="timeout"></param>
        public void WaitForShutdown(TimeSpan timeout)
        {
            if (!ShutdownRequested) throw new InvalidOperationException("Shutdown has not been requested.");
            if (!GetShutdownCompletionTask().WaitWithUnwrapExceptions(timeout)) throw new TimeoutException();
        }

        /// <summary>
        /// Wait on total shutdown of the daemon.
        /// Propagates startup failures but not shutdown failures.
        /// </summary>
        public void WaitForTermination()
        {
            GetShutdownCompletionTask().WaitWithUnwrapExceptions();
        }

        /// <summary>
        /// Wait on completion, propagating all errors.
        /// </summary>
        public void Wait(CancellationToken token)
        {
            lifetimeTask.WaitWithUnwrapExceptions(token);
        }

        /// <summary>
        /// Wait on completion, propagating all errors.
        /// </summary>
        public void Wait()
        {
            lifetimeTask.WaitWithUnwrapExceptions();
        }
    }
}
