using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.NativeMessaging
{
    public class NativeMessageQueue
    {
        private readonly INativeMessageWriter writer;

        public NativeMessageQueue(INativeMessageWriter writer)
        {
            this.writer = writer;
        }

        private readonly ConcurrentQueue<string> sendQueue = new ConcurrentQueue<string>();
        private readonly AutoResetEvent enqueued = new AutoResetEvent(false);

        public void Add(string messageJson)
        {
            sendQueue.Enqueue(messageJson);
            enqueued.Set();
        }

        /// <summary>
        /// Dispatch queued messages until the queue is empty or  until cancelled.
        /// </summary>
        public async Task DispatchPending(CancellationToken token = default(CancellationToken))
        {
            string messageJson;
            while (sendQueue.TryDequeue(out messageJson))
            {
                await writer.SendMessage(messageJson);
                token.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Dispatch queued messages until cancelled.
        /// </summary>
        public async Task RunDispatch(CancellationToken token = default(CancellationToken))
        {
            while (!token.IsCancellationRequested)
            {
                await DispatchPending(token);
                await enqueued.AsTask(token);
            }
            token.ThrowIfCancellationRequested();
        }
    }
}
