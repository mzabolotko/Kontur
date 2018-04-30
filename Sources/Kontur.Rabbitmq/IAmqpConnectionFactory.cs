using RabbitMQ.Client;
using System;

namespace Kontur.Rabbitmq
{
    public interface IAmqpConnectionFactory
    {
        IConnection CreateConnection();
    }
}
