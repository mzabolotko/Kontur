namespace Kontur.Rabbitmq
{
    public interface IAmqpDeserializer
    {
        T Deserialize<T>(AmqpMessage amqpMessage);
    }
}
