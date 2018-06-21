using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.Rabbitmq
{
    public class AmqpSubscriptionBuilder : IAmqpSubscriptionBuilder
    {
        private IAmqpSerializer defaultSerializer;
        private IAmqpRouter router;
        private IAmqpPropertyBuilder propertyBuilder;
        private IList<Func<IAmqpConnectionFactory, AmqpMessageBuilder, ISubscriptionRegistry, ISubscriptionTag>> subscriptions;
        private IAmqpConnectionFactory connectionFactory;


        public AmqpSubscriptionBuilder()
        {
            this.defaultSerializer = new SimpleSerializer();
            this.connectionFactory = new AsyncAmqpConnectionFactory(new Uri("amqp://"));
            this.router = new AmqpRouter();
            this.propertyBuilder = new AmqpPropertyBuilder();
            this.subscriptions = new List<Func<IAmqpConnectionFactory, AmqpMessageBuilder, ISubscriptionRegistry, ISubscriptionTag>>();
        }

        public IAmqpConnectionFactory ConnectionFactory => this.connectionFactory;

        public IAmqpSubscriptionBuilder RouteTo<T>(string exchangeName, string routingKey)
        {
            this.router.Register<T>(m => exchangeName, m => routingKey);
            this.subscriptions.Add(
                (connectionFactory, builder, bus) => bus.Subscribe<T>(new AmqpSender(connectionFactory, builder)));

            return this;
        }

        public IAmqpSubscriptionBuilder WithConnectionFactory(IAmqpConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;

            return this;
        }

        public ISubscriptionTag Build(ISubscriptionRegistry registry)
        {
            var serializerFactory = new AmqpSerializerFactory("plain/text", defaultSerializer);
            var amqpMessageBuilder = new AmqpMessageBuilder(propertyBuilder, router, serializerFactory);

            var subscriptionTags =
                this.subscriptions
                    .Select(createSubsriptionTag => createSubsriptionTag(this.connectionFactory, amqpMessageBuilder, registry))
                    .ToList();

            return new CompositeSubscriptionTag(Guid.NewGuid().ToString(), subscriptionTags);
        }

    }
}
