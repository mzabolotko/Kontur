using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Kontur.Rabbitmq
{
    public class SimpleSerializer : IAmqpSerializer
    {
        public byte[] Serialize(IMessage message)
        {
            return Encoding.UTF8.GetBytes(message.Payload.ToString());
        }

        public T Deserialize<T>(AmqpMessage message) where T : class
        {
            var ms = new MemoryStream(message.Payload) { Position = 0 };
            var ser = new DataContractJsonSerializer(typeof(T));

            return ser.ReadObject(ms) as T;
        }
    }
}
