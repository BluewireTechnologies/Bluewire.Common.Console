using System.IO;

namespace Bluewire.Common.NativeMessaging
{
    public class MessageTooLargeException : IOException
    {
        public long MessageLength { get; }

        public MessageTooLargeException(long messageLength) : base($"Incoming message length exceeds limit: {messageLength} bytes")
        {
            MessageLength = messageLength;
        }
    }
}
