using RabbitMQ.Client;

namespace Kontur.Rabbitmq
{
    public interface IAmqpConnectionFactory
    {
        IConnection CreateConnection();
    }
}
