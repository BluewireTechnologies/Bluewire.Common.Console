using Bluewire.Common.NativeMessaging;
using Newtonsoft.Json;

namespace NativeMessagingHarness
{
    class JsonMessageSerialisation : IMessageSerialisation
    {
        public string Serialise<T>(T message)
        {
            return JsonConvert.SerializeObject(message);
        }

        public T Deserialise<T>(string messageJson)
        {
            return JsonConvert.DeserializeObject<T>(messageJson);
        }
    }
}
