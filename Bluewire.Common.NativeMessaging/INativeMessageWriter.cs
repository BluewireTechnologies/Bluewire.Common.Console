using System.Threading.Tasks;

namespace Bluewire.Common.NativeMessaging
{
    public interface INativeMessageWriter
    {
        Task SendMessage(string messageJson);
    }
}
