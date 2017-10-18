using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bluewire.Common.NativeMessaging.UnitTests
{
    [TestFixture]
    public class NativeMessageWriterTests
    {
        [Test]
        public async Task WritesLengthAndMessage()
        {
            var stream = new MemoryStream();
            var sut = new NativeMessageWriter(stream);

            await sut.SendMessage("Test");

            Assert.That(stream.ToArray(), Is.EqualTo(TestStream.GetBytes(8, "\x4\x0\x0\x0Test")));
        }
    }
}
