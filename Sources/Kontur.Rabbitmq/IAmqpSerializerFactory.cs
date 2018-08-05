namespace Kontur.Rabbitmq
{
    public interface IAmqpSerializerFactory
    {
        IAmqpSerializer CreateSerializer(IMessage message);

        IAmqpSerializer CreateSerializer(string contentType);
    }
}