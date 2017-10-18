using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Bluewire.Common.NativeMessaging.UnitTests
{
    [TestFixture]
    public class NativeMessageReceiverTests
    {
        [Test]
        public async Task WhenMessageReadIsNull_ReturnsFalse()
        {
            var reader = Mock.Of<INativeMessageReader>(r => r.ReceiveMessage() == Task.FromResult<string>(null));
            var sut = new NativeMessageReceiver(reader, Mock.Of<IMessageSerialisation>());

            var result = await sut.Receive(Mock.Of<IMessageHandler<object>>());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task WhenMessageReadIsEmpty_ReturnsTrue()
        {
            var reader = Mock.Of<INativeMessageReader>(r => r.ReceiveMessage() == Task.FromResult(""));
            var sut = new NativeMessageReceiver(reader, Mock.Of<IMessageSerialisation>());

            var result = await sut.Receive(Mock.Of<IMessageHandler<object>>());

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task WhenMessageReadIsEmpty_NotifiesHandlerOfZeroSizeMessage()
        {
            var reader = Mock.Of<INativeMessageReader>(r => r.ReceiveMessage() == Task.FromResult(""));
            var sut = new NativeMessageReceiver(reader, Mock.Of<IMessageSerialisation>());
            var handler = Mock.Of<IMessageHandler<object>>();

            await sut.Receive(handler);

            Mock.Get(handler).Verify(h => h.ZeroLengthMessage());
        }

        [Test]
        public async Task WhenMessageIsRead_DispatchesToHandler()
        {
            var deserialisedMessage = new object();
            var reader = Mock.Of<INativeMessageReader>(r => r.ReceiveMessage() == Task.FromResult("serialised"));
            var sut = new NativeMessageReceiver(reader, Mock.Of<IMessageSerialisation>(s => s.Deserialise<object>("serialised") == deserialisedMessage));
            var handler = Mock.Of<IMessageHandler<object>>();

            await sut.Receive(handler);

            Mock.Get(handler).Verify(h => h.Handle(deserialisedMessage));
        }
    }
}
