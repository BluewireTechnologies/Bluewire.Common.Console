using System;
using System.IO;

namespace Bluewire.Common.NativeMessaging
{
    public class ConsoleNativeStreams : INativeStreams, IDisposable
    {
        public Stream StdIn { get; }
        public Stream StdOut { get; }

        public ConsoleNativeStreams()
        {
            StdIn = Console.OpenStandardInput();
            StdOut = Console.OpenStandardOutput();
        }

        public void Dispose()
        {
            try
            {
                StdIn.Dispose();
            }
            finally
            {
                StdOut.Dispose();
            }
        }
    }
}
