using System;
using NUnit.Framework;

namespace Bluewire.Common.NativeMessaging.UnitTests
{
    [TestFixture]
    public class NativeHostArgumentParserTests
    {
        [Test]
        public void ParsesOriginUriAlone()
        {
            var sut = new NativeHostArgumentParser();

            var session = sut.Parse(new [] { "chrome-extension://something" });

            Assert.That(session.Origin, Is.EqualTo(new Uri("chrome-extension://something")));
        }

        [Test, Description("Chrome 54 and earlier passed this URI as the second parameter")]
        public void ParsesOriginUriAsSecondArgument()
        {
            var sut = new NativeHostArgumentParser();

            var session = sut.Parse(new [] { "first-arg", "chrome-extension://something" });

            Assert.That(session.Origin, Is.EqualTo(new Uri("chrome-extension://something")));
        }

        [Test]
        public void ParsesSinglePartWindowHandleArgument()
        {
            var sut = new NativeHostArgumentParser();

            var session = sut.Parse(new [] { "--parent-window=424242" });

            Assert.That(session.ParentWindowHandle, Is.EqualTo(new IntPtr(424242)));
        }

        [Test]
        public void ParsesTwoPartWindowHandleArgument()
        {
            var sut = new NativeHostArgumentParser();

            var session = sut.Parse(new [] { "--parent-window", "424242" });

            Assert.That(session.ParentWindowHandle, Is.EqualTo(new IntPtr(424242)));
        }

        [TestCase("0.2")]
        [TestCase("0x0dedbeef")]
        [TestCase("0xdeadbeef")]
        [TestCase("string")]
        public void WhenParentWindowIsNotDecimalInteger_ThrowsFormatException(string notDecimalInteger)
        {
            var sut = new NativeHostArgumentParser();

            Assert.Throws<FormatException>(() => sut.Parse(new [] { "--parent-window", notDecimalInteger }));
        }
    }
}
