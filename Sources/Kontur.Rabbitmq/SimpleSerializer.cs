namespace Kontur.Rabbitmq
{
    public class SimpleSerializer : IAmqpSerializer
    {
        public byte[] Serialize(IMessage message)
        {
            return System.Text.Encoding.UTF8.GetBytes(message.Payload.ToString());
        }
    }
}
