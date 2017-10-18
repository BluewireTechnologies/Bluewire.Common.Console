using System.Threading.Tasks;

namespace Bluewire.Common.NativeMessaging
{
    public interface IMessageHandler<in T>
    {
        Task Handle(T message);
        void ZeroLengthMessage();
    }
}
