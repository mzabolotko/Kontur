namespace Kontur.Rabbitmq
{
    internal class AmqpMessageBuilder
    {
        private readonly IAmqpPropertyBuilder propertyBuilder;
        private readonly IAmqpRouter router;
        private readonly IAmqpSerializerFactory serializerFactory;

        public AmqpMessageBuilder(
            IAmqpPropertyBuilder amqpPropertyBuilder,
            IAmqpRouter router,
            IAmqpSerializerFactory serializerFactory)
        {
            this.propertyBuilder = amqpPropertyBuilder;
            this.router = router;
            this.serializerFactory = serializerFactory;
        }

        public AmqpMessage Build(IMessage message)
        {
            IAmqpProperties properties = this.propertyBuilder.BuildPropertiesFromHeaders(message.Headers);
            string exchangeName = this.router.GetExchange(message);
            string routingKey = this.router.GetRoutingKey(message);

            IAmqpSerializer amqpSerializer = this.serializerFactory.CreateSerializer(message);
            byte[] payload = amqpSerializer.Serialize(message);

            return new AmqpMessage(
                properties,
                exchangeName,
                routingKey,
                payload);
        }

    }
}
