using RabbitMQ.Client;

namespace Kontur.Rabbitmq
{
    //TODO: support mandatory publish

    internal class AmqpMessage
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

        public IAmqpProperties Properties { get; }
        public string ExchangeName { get; }
        public string RoutingKey { get; }
        public byte[] Payload { get; }
    }

}
