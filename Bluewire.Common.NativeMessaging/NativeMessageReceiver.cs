using System.Threading.Tasks;

namespace Bluewire.Common.NativeMessaging
{
    public class NativeMessageReceiver
    {
        private readonly INativeMessageReader reader;
        private readonly IMessageSerialisation serialisation;

        public NativeMessageReceiver(INativeMessageReader reader, IMessageSerialisation serialisation)
        {
            this.reader = reader;
            this.serialisation = serialisation;
        }

        /// <summary>
        /// Deserialise the next message as the specified type and try to handle it.
        /// </summary>
        /// <returns>True, until no more messages can be received (end of stream)</returns>
        public async Task<bool> Receive<T>(IMessageHandler<T> handler)
        {
            var json = await reader.ReceiveMessage();
            if (json == null) return false;
            if (json.Length == 0)
            {
                handler.ZeroLengthMessage();
                return true;
            }
            var message = serialisation.Deserialise<T>(json);
            await handler.Handle(message);
            return true;
        }
    }
}
