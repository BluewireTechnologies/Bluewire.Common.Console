namespace Bluewire.Common.NativeMessaging
{
    public class NativeMessageSender
    {
        private readonly NativeMessageQueue queue;
        private readonly IMessageSerialisation serialisation;

        public NativeMessageSender(NativeMessageQueue queue, IMessageSerialisation serialisation)
        {
            this.queue = queue;
            this.serialisation = serialisation;
        }

        public void Send<T>(T message)
        {
            var json = serialisation.Serialise(message);
            queue.Add(json);
        }
    }
}
