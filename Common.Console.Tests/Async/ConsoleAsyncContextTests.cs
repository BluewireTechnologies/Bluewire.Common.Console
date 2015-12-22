using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bluewire.Common.Console.Async;
using NUnit.Framework;

namespace Bluewire.Common.Console.Tests.Async
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
