using System.Threading.Tasks;

namespace Kontur.Rabbitmq
{
    // TODO: support mandatory publish
    public class AmqpMessage
    {
        public AmqpMessage(
            IAmqpProperties properties,
            string exchangeName,
            string routingKey,
            byte[] payload)
        {
            this.Properties = properties;
            this.ExchangeName = exchangeName;
            this.RoutingKey = routingKey;
            this.Payload = payload;
        }

        public AmqpMessage(
            IAmqpProperties properties, 
            string exchangeName, 
            string routingKey, 
            byte[] payload,
            TaskCompletionSource<bool> tcs) 
            : this(properties, exchangeName, routingKey, payload)
        {
            this.Task = tcs;
        }

        public IAmqpProperties Properties { get; }

        public string ExchangeName { get; }

        public string RoutingKey { get; }

        public byte[] Payload { get; }

        public TaskCompletionSource<bool> Task { get; }
    }
}
