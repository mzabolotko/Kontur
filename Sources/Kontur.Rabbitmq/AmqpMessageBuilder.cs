using System.Collections.Generic;

namespace Kontur.Rabbitmq
{
    internal class AmqpMessageBuilder : IAmqpMessageBuilder
    {
        private readonly IAmqpSerializerFactory serializerFactory;
        private readonly IAmqpPropertyBuilder propertyBuilder;
        private readonly IAmqpRouter router;

        public AmqpMessageBuilder(
            IAmqpSerializerFactory serializerFactory,
            IAmqpPropertyBuilder propertyBuilder,
            IAmqpRouter router)
        {
            this.serializerFactory = serializerFactory;
            this.propertyBuilder = propertyBuilder;
            this.router = router;
        }

        public IMessage Deserialize<T>(AmqpMessage amqpMessage) where T : class
        {
            IAmqpSerializer amqpDeserializer = this.serializerFactory.CreateSerializer(amqpMessage.Properties.ContentType);
            T payload = amqpDeserializer.Deserialize<T>(amqpMessage);
            IDictionary<string, string> headers = this.propertyBuilder.BuildHeadersFromProperties(amqpMessage.Properties);

            return new Message<T>(payload, new Dictionary<string, string>(headers));
        }

        public AmqpMessage Serialize(IMessage message)
        {
            IAmqpProperties properties = this.propertyBuilder.BuildPropertiesFromHeaders(message.Headers);
            string exchangeName = this.router.GetExchange(message);
            string routingKey = this.router.GetRoutingKey(message);

            IAmqpSerializer amqpSerializer = this.serializerFactory.CreateSerializer(message);
            byte[] payload = amqpSerializer.Serialize(message);

            return new AmqpMessage(properties, exchangeName, routingKey, payload);
        }
    }
}
