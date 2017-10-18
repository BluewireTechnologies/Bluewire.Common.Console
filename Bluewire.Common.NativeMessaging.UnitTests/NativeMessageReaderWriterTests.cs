using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bluewire.Common.NativeMessaging.UnitTests
{
    /// <summary>
    /// The Reader and Writer classes have StayOnCallerContext constants. When one or both of
    /// these are set to 'true', the corresponding test(s) here should fail.
    /// </summary>
    [TestFixture]
    public class NativeMessageReaderWriterThreadingTests
    {
        [Test, Timeout(5000)]
        public async Task WriterDoesNotBlockOnCallingContext()
        {
            await RunOnSingleThreadedContext(async () => {
                using (var pipe = new AnonymousPipeServerStream(PipeDirection.Out, System.IO.HandleInheritability.None, 1))
                using (var stdin = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle))
                {
                    var writer = new NativeMessageWriter(pipe);
                    var reader = new NativeMessageReader(stdin);

                    var writerTask = writer.SendMessage("Test");

                    // Block the current SynchronizationContext by hard-waiting on a Result.
                    // Don't block internals of ReceiveMessage since we're not testing that here.
                    var message = Task.Run(() => reader.ReceiveMessage()).Result;

                    Assert.That(message, Is.EqualTo("Test"));

                    await writerTask;
                }
            });
        }

        [Test, Timeout(5000)]
        public async Task ReaderDoesNotBlockOnCallingContext()
        {
            await RunOnSingleThreadedContext(async () => {
                using (var pipe = new AnonymousPipeServerStream(PipeDirection.Out, System.IO.HandleInheritability.None, 1))
                using (var stdin = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle))
                {
                    var writer = new NativeMessageWriter(pipe);
                    var reader = new NativeMessageReader(stdin);

                    // Don't block internals of SendMessage since we're not testing that here.
                    var writerTask = Task.Run(() => writer.SendMessage("Test"));

                    // Block the current SynchronizationContext by hard-waiting on a Result.
                    var message = reader.ReceiveMessage().Result;

                    Assert.That(message, Is.EqualTo("Test"));

                    await writerTask;
                }
            });
        }

        /// <summary>
        /// Force the operation to run in a single-threaded SynchronizationContext. Hard waits will block
        /// asynchronous operations waiting to continue on their caller's context.
        /// </summary>
        /// <returns></returns>
        private static async Task RunOnSingleThreadedContext(Func<Task> operation)
        {
            var scheduler = new ConcurrentExclusiveSchedulerPair(TaskScheduler.Current, 1);
            var factory = new TaskFactory(scheduler.ExclusiveScheduler);
            await await factory.StartNew(operation);
        }
    }
}
