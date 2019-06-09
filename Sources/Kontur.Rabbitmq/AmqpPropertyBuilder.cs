using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Kontur.Rabbitmq
{
    internal class AmqpPropertyBuilder : IAmqpPropertyBuilder
    {
        public static readonly string ContentEncoding = "content-encoding";
        public static readonly string ContentType = "content-type";
        public static readonly string CorrelationId = "correlation-id";
        public static readonly string MessageId = "message-id";
        public static readonly string Persistent = "persistent";
        public static readonly string ReplyTo = "reply-to";

        public IAmqpProperties BuildPropertiesFromHeaders(IReadOnlyDictionary<string, string> headers)
        {
            var properties = new AmqpProperties();

            properties.ContentEncoding = GetHeaderOrNull(headers, ContentEncoding);
            properties.ContentType = GetHeaderOrNull(headers, ContentType);
            properties.CorrelationId = GetHeaderOrNull(headers, CorrelationId);
            properties.MessageId = GetHeaderOrNull(headers, MessageId);
            properties.Persistent = bool.TryParse(GetHeaderOrNull(headers, Persistent), out bool result);
            properties.ReplyTo = GetHeaderOrNull(headers, ReplyTo);

            var presetHeaders = new List<string> {
                ContentEncoding,
                ContentType,
                CorrelationId,
                MessageId,
                Persistent,
                ReplyTo
            };

            properties.Headers =
                headers
                .Where(kv => !presetHeaders.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return properties;
        }

        public IAmqpProperties BuildPropertiesFromProperties(IBasicProperties basicProperties)
        {
            var properties = new AmqpProperties();

            properties.ContentEncoding = basicProperties.ContentEncoding;
            properties.ContentType = basicProperties.ContentType;
            properties.CorrelationId = basicProperties.CorrelationId;
            properties.MessageId = basicProperties.MessageId;
            properties.Persistent = basicProperties.Persistent;
            properties.ReplyTo = basicProperties.ReplyTo;
            properties.Headers = basicProperties.Headers.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

            return properties;
        }

        public IDictionary<string, string> BuildHeadersFromProperties(IAmqpProperties amqpProperties)
        {
            var headers = new Dictionary<string, string>(amqpProperties.Headers);
            headers.Add(ContentEncoding, amqpProperties.ContentEncoding);
            headers.Add(ContentType, amqpProperties.ContentType);
            headers.Add(CorrelationId, amqpProperties.CorrelationId);
            headers.Add(MessageId, amqpProperties.MessageId);
            headers.Add(Persistent, amqpProperties.Persistent.ToString());
            headers.Add(ReplyTo, amqpProperties.ReplyTo);
            return headers;
        }

        private string GetHeaderOrNull(IReadOnlyDictionary<string, string> headers, string headerName)
        {
            return headers.ContainsKey(headerName)
                ? headers[headerName]
                : null;
        }
    }

}
