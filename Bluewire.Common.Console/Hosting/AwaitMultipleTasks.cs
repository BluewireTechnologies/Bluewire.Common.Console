using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.Console.Hosting
{
    public class AwaitMultipleTasks
    {
        private readonly TaskCompletionSource<object> completion = new TaskCompletionSource<object>();
        private readonly CountdownEvent countdown = new CountdownEvent(1); // '1' prevents it completing immediately.
        private int isWaiting;

        public void Track(Task shutdownTask)
        {
            if (countdown.IsSet) throw new InvalidOperationException("All tasks have completed.");
            countdown.AddCount();
            shutdownTask.ContinueWith(OnCompleteCallback, TaskContinuationOptions.ExecuteSynchronously);
        }

        private void OnCompleteCallback(Task task)
        {
            Decrement();
        }

        private void Decrement()
        {
            if (countdown.Signal())
            {
                completion.SetResult(null);
            }
        }

        public Task GetWaitTask()
        {
            if (Interlocked.Increment(ref isWaiting) == 1) Decrement(); // Counteract the initial '1', but only once.
            return completion.Task;
        }
    }
}
