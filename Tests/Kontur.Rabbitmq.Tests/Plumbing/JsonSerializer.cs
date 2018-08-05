namespace Kontur.Rabbitmq.Tests.Plumbing
{
    public class JsonSerializer : IAmqpSerializer
    {
        public T Deserialize<T>(AmqpMessage message) where T : class
        {
            throw new System.NotImplementedException();
        }

        public byte[] Serialize(IMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}