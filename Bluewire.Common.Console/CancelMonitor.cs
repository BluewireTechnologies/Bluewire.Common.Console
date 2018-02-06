using System;
using System.Threading;
using System.Threading.Tasks;
using Bluewire.Common.Console.Logging;

namespace Bluewire.Common.Console
{
    public class CancelMonitor
    {
        private readonly ManualResetEvent terminationEvent = new ManualResetEvent(false);

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        public CancellationToken GetToken()
        {
            return tokenSource.Token;
        }

        public CancelMonitor()
        {
            System.Console.CancelKeyPress += RequestCancel;
        }

        public EventHandler<ConsoleCancelEventArgs> CancelRequested = (s, e) => { };
        public EventHandler<ConsoleCancelEventArgs> KillRequested = (s, e) => { };

        /// <summary>
        /// Attaches default console message loggers to Cancel and Kill events.
        /// </summary>
        public void LogRequestsToConsole()
        {
            CancelRequested += (s, e) => Log.Console.Warn("Shutting down.");
            KillRequested += (s, e) => Log.Console.Warn("CTRL-C pressed twice. Terminating.");
        }

        public void RequestCancel(object sender, ConsoleCancelEventArgs args)
        {
            if (tokenSource.IsCancellationRequested)
            {
                KillRequested(sender, args);
                return;
            }
            tokenSource.Cancel();
            terminationEvent.Set();
            args.Cancel = true;
            CancelRequested(sender, args);
        }

        public void CheckForCancel()
        {
            if (tokenSource.IsCancellationRequested)
            {
                throw new ErrorWithReturnCodeException(255, "Operation was aborted via Ctrl-C.");
            }
        }

        public void BreakOnCancel(ParallelLoopState state)
        {
            if (tokenSource.IsCancellationRequested) state.Break();
        }

        public bool WaitForCancel(TimeSpan timeout)
        {
            return terminationEvent.WaitOne(timeout);
        }

        public void WaitForCancel()
        {
            terminationEvent.WaitOne();
        }
    }
}
