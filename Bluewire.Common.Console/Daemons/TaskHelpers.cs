using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.Console.Daemons
{
    internal static class TaskHelpers
    {
        public static Task AsTask(this WaitHandle waitHandle, CancellationToken token = default(CancellationToken))
        {
            return AsTask(waitHandle, TimeSpan.FromMilliseconds(Int32.MaxValue), token);
        }

        public static async Task AsTask(this WaitHandle waitHandle, TimeSpan limit, CancellationToken token = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<object>();
            using (token.Register(() => tcs.TrySetCanceled()))
            {
                ThreadPool.RegisterWaitForSingleObject(
                    waitHandle,
                    (o, timeout) =>
                    {
                        if (timeout) tcs.TrySetException(new TimeoutException());
                        else tcs.TrySetResult(null);
                    },
                    null,
                    limit,
                    true);

                await tcs.Task.ConfigureAwait(false);
            }
        }

        public static Task GetCompletionOnlyTask(this Task task)
        {
            if (task.IsCompleted) return Task.FromResult<object>(null);
            var asyncObject = (IAsyncResult)task;
            return asyncObject.AsyncWaitHandle.AsTask();
        }

        public static void WaitWithUnwrapExceptions(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

        public static bool WaitWithUnwrapExceptions(this Task task, TimeSpan timeout)
        {
            try
            {
                return task.Wait(timeout);
            }
            catch
            {
                // This should rethrow the exception, unwrapped:
                task.GetAwaiter().GetResult();
                return true;
            }
        }
    }
}
