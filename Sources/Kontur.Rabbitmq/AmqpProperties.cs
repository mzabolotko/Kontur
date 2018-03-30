using RabbitMQ.Client;
using System.Collections.Generic;

namespace Kontur.Rabbitmq
{
    internal class AmqpProperties : IAmqpProperties
    {
        public string ReplyTo { get; set; }
        public bool Persistent { get; set; }
        public string MessageId { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public string CorrelationId { get; set; }
        public string ContentType { get; set; }
        public string ContentEncoding { get; set; }

        public void CopyTo(IBasicProperties basicProperties)
        {
            basicProperties.Headers = new Dictionary<string, object>();
            foreach (var header in this.Headers)
            {
                basicProperties.Headers.Add(header.Key, header.Value);
            }

            if (this.ReplyTo != null)
            {
                basicProperties.ReplyTo = this.ReplyTo;
            }

            basicProperties.Persistent = this.Persistent;
            basicProperties.DeliveryMode = (byte)(Persistent ? 2 : 1);

            if (this.MessageId != null)
            {
                basicProperties.MessageId = this.MessageId;
            }

            if (this.CorrelationId != null)
            {
                basicProperties.CorrelationId = this.CorrelationId;
            }

            if (this.ContentType != null)
            {
                basicProperties.ContentType = this.ContentType;
            }

            if (this.ContentEncoding != null)
            {
                basicProperties.ContentEncoding = this.ContentEncoding;
            }
        }
    }
}