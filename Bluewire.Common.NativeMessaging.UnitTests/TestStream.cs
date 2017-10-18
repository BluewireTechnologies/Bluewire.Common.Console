using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Bluewire.Common.NativeMessaging.UnitTests
{
    internal static class TestStream
    {
        public static byte[] GetBytes(int expectedBytesTotal, string text)
        {
            Assume.That(BitConverter.IsLittleEndian);
            var bytes = Encoding.UTF8.GetBytes(text);
            Assume.That(bytes.Length, Is.EqualTo(expectedBytesTotal));
            return bytes;
        }

        public static Stream Create(int expectedBytesTotal, string text)
        {
            return new MemoryStream(GetBytes(expectedBytesTotal, text));
        }
    }
}
