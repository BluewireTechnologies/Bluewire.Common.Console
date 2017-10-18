using System.Threading.Tasks;
using Bluewire.Common.NativeMessaging;
using Newtonsoft.Json.Linq;

namespace NativeMessagingHarness
{
    class EchoService
    {
        private readonly NativeHostSessionArguments session;

        public EchoService(NativeHostSessionArguments session)
        {
            this.session = session;
        }

        public async Task Run()
        {
            using (var streams = new ConsoleNativeStreams())
            {
                var reader = new NativeMessageReader(streams.StdIn);
                var writer = new NativeMessageWriter(streams.StdOut);
                var queue = new NativeMessageQueue(writer);

                var receiver = new NativeMessageReceiver(reader, new JsonMessageSerialisation());
                var sender = new NativeMessageSender(queue, new JsonMessageSerialisation());

                sender.Send(new { Origin = session.Origin?.ToString(), ParentWindowHandle = session.ParentWindowHandle?.ToInt32() });

                var handler = new EchoHandler(sender);

                while (await receiver.Receive(handler))
                {
                    await queue.DispatchPending();
                }
            }
        }

        class EchoHandler : IMessageHandler<JObject>
        {
            private readonly NativeMessageSender sender;

            public EchoHandler(NativeMessageSender sender)
            {
                this.sender = sender;
            }

            public Task Handle(JObject message)
            {
                sender.Send(new { Received = message });
                return Task.FromResult<object>(null);
            }

            public void ZeroLengthMessage()
            {
                sender.Send(new { Received = "(zero-length message)" });
            }
        }
    }
}
