namespace Kontur.Rabbitmq
{
    internal interface IAmqpMessageBuilder
    {
        IMessage Deserialize<T>(AmqpMessage amqpMessage) where T : class;

        AmqpMessage Serialize(IMessage message);
    }
}