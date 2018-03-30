namespace Kontur.Rabbitmq
{
    internal interface IAmqpSerializerFactory
    {
        IAmqpSerializer CreateSerializer(IMessage message);
    }
}