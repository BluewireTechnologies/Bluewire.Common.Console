using System.Threading.Tasks;

namespace Bluewire.Common.NativeMessaging
{
    public interface INativeMessageReader
    {
        Task<string> ReceiveMessage();
    }
}
