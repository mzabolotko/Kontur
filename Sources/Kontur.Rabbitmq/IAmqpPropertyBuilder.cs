
using RabbitMQ.Client;

namespace Kontur.Rabbitmq
{

    internal interface IAmqpPropertyBuilder
    {
        IAmqpProperties Build(IMessage message);
    }
}