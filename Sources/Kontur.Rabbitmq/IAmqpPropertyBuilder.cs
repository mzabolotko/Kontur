using RabbitMQ.Client;
using System.Collections.Generic;

namespace Kontur.Rabbitmq
{
    internal interface IAmqpPropertyBuilder
    {
        IAmqpProperties BuildPropertiesFromHeaders(IReadOnlyDictionary<string, string> headers);

        IAmqpProperties BuildPropertiesFromProperties(IBasicProperties basicProperties);

        IDictionary<string, string> BuildHeadersFromProperties(IAmqpProperties amqpProperties);
    }
}