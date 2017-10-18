using System.IO;

namespace Bluewire.Common.NativeMessaging
{
    public interface INativeStreams
    {
        Stream StdIn { get; }
        Stream StdOut { get; }
    }
}
