using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.Console.Daemons
{
    internal static class TaskHelpers
    {
        public static Task AsTask(this WaitHandle waitHandle, CancellationToken token = default(CancellationToken))
        {
            // Infinite timeout:
            return AsTask(waitHandle, -1, token);
        }

        public static Task AsTask(this WaitHandle waitHandle, TimeSpan limit, CancellationToken token = default(CancellationToken))
        {
            return AsTask(waitHandle, (long)limit.TotalMilliseconds, token);
        }

        private static async Task AsTask(WaitHandle waitHandle, long limitMilliseconds, CancellationToken token = default(CancellationToken))
        {
            // Complete immediately if possible.
            // Simplifies mocking a single-threaded TaskScheduler, since it removes the need to tolerate continuations
            // being queued from other threads eg. the threadpool.
            token.ThrowIfCancellationRequested();
            if (waitHandle.WaitOne(TimeSpan.Zero))
            {
                // In case of races when waiting on the CancellationToken's WaitHandle:
                token.ThrowIfCancellationRequested();
                return;
            }

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
                    limitMilliseconds,
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

        public static bool WaitWithUnwrapExceptions(this Task task, CancellationToken token)
        {
            try
            {
                task.Wait(token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
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
