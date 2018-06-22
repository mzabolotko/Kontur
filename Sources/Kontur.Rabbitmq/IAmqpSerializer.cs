namespace Kontur.Rabbitmq
{
    public interface IAmqpSerializer
    {
        byte[] Serialize(IMessage message);
    }
}