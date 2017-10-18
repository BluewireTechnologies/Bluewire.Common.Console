namespace Bluewire.Common.NativeMessaging
{
    public interface IMessageSerialisation
    {
        string Serialise<T>(T message);
        T Deserialise<T>(string messageJson);
    }
}
