using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bluewire.Common.NativeMessaging.UnitTests
{
    [TestFixture]
    public class NativeMessageReaderTests
    {
        [Test]
        public async Task WhenMessageIsPresent_ReturnsMessage()
        {
            var stream = TestStream.Create(8, "\x4\x0\x0\x0Test");
            var sut = new NativeMessageReader(stream);

            var message = await sut.ReceiveMessage();

            Assert.That(message, Is.EqualTo("Test"));
        }

        [Test]
        public async Task WhenAtEndOfStream_ReturnsNull()
        {
            var stream = TestStream.Create(10, "some bytes");
            stream.Seek(0, SeekOrigin.End);
            var sut = new NativeMessageReader(stream);

            var message = await sut.ReceiveMessage();

            Assert.That(message, Is.Null);
        }

        [Test]
        public async Task WhenZeroLengthMessage_ReturnsEmptyString()
        {
            var stream = TestStream.Create(4, "\x0\x0\x0\x0");
            var sut = new NativeMessageReader(stream);

            var message = await sut.ReceiveMessage();

            Assert.That(message, Is.EqualTo(""));
        }

        [Test]
        public void WhenEndOfStreamEncounteredInHeader_ThrowsEndOfStreamException()
        {
            var stream = TestStream.Create(2, "\x0\x0");  // Expect 4-byte header, only 2 bytes present.
            var sut = new NativeMessageReader(stream);

            Assert.ThrowsAsync<EndOfStreamException>(() => sut.ReceiveMessage());
        }

        [Test]
        public void WhenEndOfStreamEncounteredInMessage_ThrowsEndOfStreamException()
        {
            var stream = TestStream.Create(8, "\xC\x0\x0\x0Part");  // Expect 12-byte message, only 4 bytes present.
            var sut = new NativeMessageReader(stream);

            Assert.ThrowsAsync<EndOfStreamException>(() => sut.ReceiveMessage());
        }

        [Test]
        public void WhenMessageTooLarge_ThrowsMessageTooLargeException()
        {
            var stream = TestStream.Create(32, "\x1C\x0\x0\x0MessageMessageMessageMessage");  // 28-byte message.
            var sut = new NativeMessageReader(stream, 16);  // 16-byte message limit.

            Assert.ThrowsAsync<MessageTooLargeException>(() => sut.ReceiveMessage());
        }

        [Test]
        public void WhenMessageTooLarge_SeeksToEndOfMessage()
        {
            var stream = TestStream.Create(40, "\x1C\x0\x0\x0MessageMessageMessageMessage\x4\x0\x0\x0Test");  // 28-byte message followed by 4-byte message.
            var sut = new NativeMessageReader(stream, 16);  // 16-byte message limit.

            Assert.CatchAsync(() => sut.ReceiveMessage());

            Assert.That(stream.Position, Is.EqualTo(32));
        }

        [Test]
        public void WhenMessageTooLarge_AndContainsEndOfStream_ThrowsEndOfStreamException()
        {
            var stream = TestStream.Create(18, "\x1C\x0\x0\x0MessageMessage");  // 28-byte message, only 14 bytes present.
            var sut = new NativeMessageReader(stream, 16);  // 16-byte message limit.

            Assert.ThrowsAsync<EndOfStreamException>(() => sut.ReceiveMessage());
        }
    }
}
