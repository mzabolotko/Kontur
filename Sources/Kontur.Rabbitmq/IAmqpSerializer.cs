namespace Kontur.Rabbitmq
{
    public interface IAmqpSerializer
    {
        byte[] Serialize(IMessage message);

        T Deserialize<T>(AmqpMessage message) where T : class;
    }
}