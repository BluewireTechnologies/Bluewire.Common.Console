using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bluewire.Common.NativeMessaging
{
    public class NativeMessageReader : INativeMessageReader
    {
        /// <summary>
        /// If true, ReceiveMessage may stall if the calling context is stuck in a hard wait.
        /// </summary>
        private const bool StayOnCallerContext = false;

        private readonly Stream stdin;
        /// <summary>
        /// Limit on the size of accepted messages. Default: 1MB.
        /// </summary>
        private readonly int incomingMessageSizeLimit;

        public NativeMessageReader(Stream stdin, int incomingMessageSizeLimit = 1024*1024)
        {
            if (stdin == null) throw new ArgumentNullException(nameof(stdin));
            if (!stdin.CanRead) throw new ArgumentException("StdIn is not readable.");
            this.stdin = stdin;
            this.incomingMessageSizeLimit = incomingMessageSizeLimit;
        }

        /// <summary>
        /// Receive and return the next message.
        /// </summary>
        /// <remarks>
        /// This must exit the context so that the caller is not blocked again until a complete
        /// message has been received, even if the entire message needs to be consumed to
        /// unblock the sender in the case where the intervening buffer is too small.
        /// </remarks>
        public async Task<string> ReceiveMessage()
        {
            var lengthBuffer = new byte[4];
            var bytesRead = await stdin.ReadAsync(lengthBuffer, 0, lengthBuffer.Length).ConfigureAwait(StayOnCallerContext);
            if (bytesRead == 0) return null;    // End of stream. No more messages.
            if (bytesRead < lengthBuffer.Length) throw new EndOfStreamException("End of stream while reading message length");

            var messageLength = BitConverter.ToUInt32(lengthBuffer, 0);
            if (messageLength == 0) return "";

            return await ReadMessage(messageLength).ConfigureAwait(StayOnCallerContext);
        }

        private async Task<string> ReadMessage(long messageLength)
        {
            Debug.Assert(messageLength > 0);
            if (messageLength > incomingMessageSizeLimit)
            {
                await DiscardBytes(messageLength).ConfigureAwait(StayOnCallerContext);
                throw new MessageTooLargeException(messageLength);
            }

            var messageBuffer = new byte[messageLength];
            var bytesRead = await stdin.ReadAsync(messageBuffer, 0, messageBuffer.Length).ConfigureAwait(StayOnCallerContext);
            if (bytesRead < messageBuffer.Length) throw new EndOfStreamException("End of stream while reading message");

            return Encoding.UTF8.GetString(messageBuffer);
        }

        private async Task DiscardBytes(long count)
        {
            // Can't rely on being able to seek if eg. reading from STDIN, so read and
            // discard bytes to make up the desired total.
            var discardBuffer = new byte[4096];
            while (count > 0)
            {
                var bytesRead = await stdin.ReadAsync(discardBuffer, 0, (int)Math.Min(count, discardBuffer.Length)).ConfigureAwait(StayOnCallerContext);
                if (bytesRead <= 0)  throw new EndOfStreamException("End of stream while reading message");
                count -= bytesRead;
            }
        }
    }
}
