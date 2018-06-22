using System.Text;

namespace Kontur.Rabbitmq
{
    public class SimpleSerializer : IAmqpSerializer
    {
        public byte[] Serialize(IMessage message)
        {
            return Encoding.UTF8.GetBytes(message.Payload.ToString());
        }
    }
}
