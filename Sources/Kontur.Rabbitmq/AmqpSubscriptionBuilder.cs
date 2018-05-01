using RabbitMQ.Client;
using System;
using System.Threading.Tasks.Dataflow;

namespace Kontur.Rabbitmq
{
    public class AmqpSubscriptionBuilder : IAmqpSubscriptionBuilder
    {
        private IAmqpSerializer defaultSerializer;
        private IAmqpRouter router;
        private IAmqpPropertyBuilder propertyBuilder;

        public AmqpSubscriptionBuilder()
        {
            this.defaultSerializer = new SimpleSerializer();
            this.router = new AmqpRouter();
            this.propertyBuilder = new AmqpPropertyBuilder();
        }

        public ISubscriptionTag Build<T>(Bus bus)
        {
            IAmqpSerializerFactory serializerFactory = new AmqpSerializerFactory("plain/text", defaultSerializer);
            AmqpMessageBuilder amqpMessageBuilder = new AmqpMessageBuilder(propertyBuilder, router, serializerFactory);

            TransformBlock<IMessage, AmqpMessage> amqpBuilderBlock =
                new TransformBlock<IMessage, AmqpMessage>(
                    (Func<IMessage, AmqpMessage>)amqpMessageBuilder.Build);

            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://localhost");

            IConnection connection = factory.CreateConnection();
            IModel model = connection.CreateModel();

            AmqpSender amqpSender = new AmqpSender(model);

            ActionBlock<AmqpMessage> amqpSenderBlock =
                new ActionBlock<AmqpMessage>(
                    (Action<AmqpMessage>)amqpSender.Send);

            amqpBuilderBlock.LinkTo(amqpSenderBlock);

            return new AmqpSubscriptionTag(bus.Subscribe<T>(amqpBuilderBlock), connection, model);
        }

        public IAmqpSubscriptionBuilder RouteTo<T>(string exchangeName, string routingKey)
        {
            this.router.Register<T>(m => exchangeName, m => routingKey);

            return this;
        }

    }
}
