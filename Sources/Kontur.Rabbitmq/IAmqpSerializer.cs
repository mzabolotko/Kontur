namespace Kontur.Rabbitmq
{
    internal interface IAmqpSerializer
    {
        byte[] Serialize(IMessage message);
    }
}