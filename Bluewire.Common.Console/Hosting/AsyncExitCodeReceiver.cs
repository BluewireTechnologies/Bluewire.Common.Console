using System;
using System.Threading.Tasks;

namespace Bluewire.Common.Console.Hosting
{
    public class AsyncExitCodeReceiver : MarshalByRefObject
    {
        private readonly TaskCompletionSource<int> task = new TaskCompletionSource<int>();

        public void ExitCode(int exitCode)
        {
            task.SetResult(exitCode);
        }

        public void Exception(Exception ex)
        {
            task.SetException(ex);
        }

        public Task<int> Task { get { return task.Task; } }
    }
}
