using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.Console.Daemons
{
    public interface IHostedDaemonInfo
    {
        string Name { get; }
    }

    public interface IHostedDaemonInstance : IHostedDaemonInfo
    {
        /// <summary>
        /// Request that the monitored daemon instance should shut down.
        /// </summary>
        /// <remarks>
        /// This method MAY NEVER throw exceptions.
        /// </remarks>
        Task RequestShutdown();
        /// <summary>
        /// Wait the specified time for shutdown to complete. If the timeout
        /// expires, throws a TimeoutException.
        /// </summary>
        /// <remarks>
        /// There is no guarantee that this method will be called after RequestShutdown.
        /// Nothing is required to call it after RequestShutdown.
        /// </remarks>
        /// <param name="timeout"></param>
        void WaitForShutdown(TimeSpan timeout);
    }
}