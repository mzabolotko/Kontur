using RabbitMQ.Client;
using System.Collections.Generic;

namespace Kontur.Rabbitmq
{
    internal interface IAmqpProperties
    {
        string ReplyTo { get; }
        bool Persistent { get; }
        string MessageId { get; }
        IDictionary<string, string> Headers { get; }
        string CorrelationId { get; }
        string ContentType { get; }
        string ContentEncoding { get; }

        void CopyTo(IBasicProperties basicProperties);
    }
}