using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.Console.Async
{
    /// <summary>
    /// Implements a simple 'message loop' for asynchronous CLI applications.
    /// </summary>
    /// <remarks>
    /// See http://stackoverflow.com/questions/9208921/async-on-main-method-of-console-app
    /// </remarks>
    public class ConsoleAsyncContext : IDisposable
    {
        private readonly ConsoleTaskScheduler scheduler;
        private readonly TaskFactory factory;
        private readonly BlockingCollection<TaskItem> queue = new BlockingCollection<TaskItem>();
        private int numberOfOperations;

        class TaskItem
        {
            public Task Task { get; set; }
            public bool RethrowOnContext { get; set; }
        }

        private ConsoleAsyncContext()
        {
            scheduler = new ConsoleTaskScheduler(this);
            factory = new TaskFactory(scheduler);
        }

        public static T Run<T>(Func<Task<T>> app)
        {
            using (var context = new ConsoleAsyncContext())
            {
                return context.RunInternal(app);
            }
        }

        private T RunInternal<T>(Func<Task<T>> app)
        {
            // This is tricky. Unwrap() returns a task proxy, which must also be executed within the context.
            TaskAdded();
            var task = factory.StartNew(app, TaskCreationOptions.DenyChildAttach).Unwrap();
            ObserveTaskCompletion(task);
            RunToCompletion();
            return task.GetAwaiter().GetResult();
        }

        private void Enqueue(Task task, bool rethrowOnContext)
        {
            var item = new TaskItem { Task = task, RethrowOnContext = rethrowOnContext };
            TaskAdded();
            ObserveTaskCompletion(item.Task);
            // When does this fail?
            queue.TryAdd(item);
        }

        private void TaskAdded()
        {
            Interlocked.Increment(ref numberOfOperations);
        }
        
        private void ObserveTaskCompletion(Task task)
        {
            // If we finished the last task, don't expect any more. Return control to Main(...).
            task.ContinueWith(
                _ => { if (Interlocked.Decrement(ref numberOfOperations) == 0) queue.CompleteAdding(); },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        private void RunToCompletion()
        {
            var currentContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(new ConsoleSynchronizationContext(this));
                foreach (var item in queue.GetConsumingEnumerable())
                {
                    scheduler.RunTaskInContext(item.Task);
                    if (item.RethrowOnContext)
                    {
                        item.Task.GetAwaiter().GetResult();
                    }
                }
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(currentContext);
            }
        }

        private bool IsInContext()
        {
            return (SynchronizationContext.Current as ConsoleSynchronizationContext)?.ConsoleAsyncContext == this;
        }

        class ConsoleSynchronizationContext : SynchronizationContext
        {
            public ConsoleAsyncContext ConsoleAsyncContext { get; }

            public ConsoleSynchronizationContext(ConsoleAsyncContext asyncContext)
            {
                this.ConsoleAsyncContext = asyncContext;
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                ConsoleAsyncContext.Enqueue(ConsoleAsyncContext.factory.StartNew(() => d(state), TaskCreationOptions.DenyChildAttach), true);
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                if (ConsoleAsyncContext.IsInContext())
                {
                    d(state);
                }
                else
                {
                    var task = ConsoleAsyncContext.factory.StartNew(() => d(state), TaskCreationOptions.DenyChildAttach);
                    task.GetAwaiter().GetResult();
                }
            }
        }

        class ConsoleTaskScheduler : TaskScheduler
        {
            private readonly ConsoleAsyncContext consoleAsyncContext;

            public ConsoleTaskScheduler(ConsoleAsyncContext consoleAsyncContext)
            {
                this.consoleAsyncContext = consoleAsyncContext;
            }

            public override int MaximumConcurrencyLevel => 1;

            protected override void QueueTask(Task task)
            {
                consoleAsyncContext.Enqueue(task, false);
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (!consoleAsyncContext.IsInContext()) return false;
                return TryExecuteTask(task);
            }

            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return consoleAsyncContext.queue.Select(i => i.Task);
            }

            public void RunTaskInContext(Task task)
            {
                TryExecuteTask(task);
            }
        }

        public void Dispose()
        {
            queue.Dispose();
        }
    }
}