using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.Console
{
    public interface IDaemonisable
    {
        string Name { get; }

        /// <summary>
        /// Create a new instance of the daemon and return it.
        /// Shutdown will be performed by disposing the instance.
        /// </summary>
        /// <remarks>
        /// If shutdown is requested before the daemon is fully started, the token will be cancelled.
        /// * If the returned task completes then its result will be disposed of immediately, so returning
        ///   the constructed instance is an acceptable response to cancellation.
        /// * If the task cancels, it must perform all clean-up itself beforehand.
        /// </remarks>
        Task<IDaemon> Start(CancellationToken token);
        string[] GetDependencies();
    }
}
