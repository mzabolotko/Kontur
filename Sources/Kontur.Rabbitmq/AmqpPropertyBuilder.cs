using System.Collections.Generic;
using System.Linq;

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

        public IAmqpProperties Build(IMessage message)
        {
            var properties = new AmqpProperties();

            IReadOnlyDictionary<string, string> headers = message.Headers;

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

        private string GetHeaderOrNull(IReadOnlyDictionary<string, string> headers, string headerName)
        {
            return headers.ContainsKey(headerName)
                ? headers[headerName]
                : null;
        }
    }

}
