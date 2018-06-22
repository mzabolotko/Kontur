namespace Kontur.Rabbitmq.Tests.Plumbing
{
    public class JsonSerializer : IAmqpSerializer
    {
        public byte[] Serialize(IMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}