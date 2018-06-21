namespace Kontur.Rabbitmq
{
    internal interface IMessageBuilder
    {
        IMessage Build<T>(AmqpMessage amqpMessage);
    }
}