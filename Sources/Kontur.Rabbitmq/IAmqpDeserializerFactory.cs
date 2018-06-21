namespace Kontur.Rabbitmq
{
    public interface IAmqpDeserializerFactory
    {
        IAmqpDeserializer CreateDeserializer(AmqpMessage amqpMessage);
    }
}
