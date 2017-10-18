using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bluewire.Common.NativeMessaging
{
    public class NativeMessageWriter : INativeMessageWriter
    {
        /// <summary>
        /// If true, SendMessage may stall if the calling context is stuck in a hard wait.
        /// </summary>
        private const bool StayOnCallerContext = false;

        private readonly Stream stdout;
        /// <summary>
        /// Limit specified by the protocol: messages sent to the browser may not exceed 1MB in size.
        /// </summary>
        private const int OutgoingMessageSizeLimit = 1024*1024;

        public NativeMessageWriter(Stream stdout)
        {
            if (stdout == null) throw new ArgumentNullException(nameof(stdout));
            if (!stdout.CanWrite) throw new ArgumentException("StdOut is not writable.");
            this.stdout = stdout;
        }

        /// <summary>
        /// Send a single message.
        /// </summary>
        /// <remarks>
        /// This must exit the context, so that the caller is not blocked again until the complete
        /// message has been sent, rather than waiting on the other end to read it if the intervening
        /// buffer is not large enough.
        /// </remarks>
        public async Task SendMessage(string messageJson)
        {
            var messageBuffer = Encoding.UTF8.GetBytes(messageJson);
            if (messageBuffer.Length > OutgoingMessageSizeLimit) throw new MessageTooLargeException(messageBuffer.Length);

            var lengthBuffer = BitConverter.GetBytes(messageBuffer.Length);
            Debug.Assert(lengthBuffer.Length == 4);

            await stdout.WriteAsync(lengthBuffer, 0, lengthBuffer.Length).ConfigureAwait(StayOnCallerContext);
            await stdout.WriteAsync(messageBuffer, 0, messageBuffer.Length).ConfigureAwait(StayOnCallerContext);
        }
    }
}
