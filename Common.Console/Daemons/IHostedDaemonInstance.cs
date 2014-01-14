using System;

namespace Bluewire.Common.Console.Daemons
{
    public interface IHostedDaemonInstance
    {
        /// <summary>
        /// Request that the monitored daemon instance should shut down.
        /// </summary>
        /// <remarks>
        /// This method MAY NEVER throw exceptions.
        /// </remarks>
        void RequestShutdown();
        /// <summary>
        /// Wait the specified time for shutdown to complete. If the timeout
        /// expires, throws a TimeoutException.
        /// </summary>
        /// <remarks>
        /// There is no guarantee that this method will be called after RequestShutdown.
        /// If more than one instance is being waited on, an earlier exception might prevent
        /// this one being waited upon. Therefore, do no critical processing in this method.
        /// </remarks>
        /// <param name="timeout"></param>
        void WaitForShutdown(TimeSpan timeout);
    }
}