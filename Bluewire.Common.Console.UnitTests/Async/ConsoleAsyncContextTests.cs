using System;
using System.Threading.Tasks;
using Bluewire.Common.Console.Async;
using NUnit.Framework;

namespace Bluewire.Common.Console.UnitTests.Async
{
    [TestFixture]
    public class ConsoleAsyncContextTests
    {
        [Test]
        public void CanWaitSynchronouslyOnAsyncOperation()
        {
            var result = ConsoleAsyncContext.Run(async () =>
            {
                await Task.Delay(5);
                await Task.Yield();
                return await Task.Run(() => 42);
            });

            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void DoesNotHangWhenAwaitingATaskCompletionSource()
        {
            var tcs = new TaskCompletionSource<int>();

            Task.Delay(100).ContinueWith(_ => tcs.TrySetResult(42));

            var result = ConsoleAsyncContext.Run(async () => await tcs.Task);

            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void UnwrapsExceptions()
        {
            Assert.Throws<OutOfMemoryException>(() =>
                ConsoleAsyncContext.Run<int>(async () =>
                {
                    await Task.Yield();
                    throw new OutOfMemoryException();
                }));
        }
    }
}
