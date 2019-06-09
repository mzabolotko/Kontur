using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.Rabbitmq
{
    public class AmqpSubscriptionBuilder : IAmqpSubscriptionBuilder
    {
        private readonly IAmqpRouter router;

        private readonly IAmqpPropertyBuilder propertyBuilder;

        private readonly IList<Func<IAmqpConnectionFactory, IAmqpMessageBuilder, ISubscriptionRegistry, ISubscriptionTag>> subscriptions;

        public IAmqpConnectionFactory ConnectionFactory { get; private set; }

        public IDictionary<string, IAmqpSerializer> Serializers { get; }

        public AmqpSubscriptionBuilder()
        {
            this.Serializers = new Dictionary<string, IAmqpSerializer>
            {
                { "plain/text", new SimpleSerializer() }
            };
            this.ConnectionFactory = new AsyncAmqpConnectionFactory(new Uri("amqp://"));
            this.router = new AmqpRouter();
            this.propertyBuilder = new AmqpPropertyBuilder();
            this.subscriptions = new List<Func<IAmqpConnectionFactory, IAmqpMessageBuilder, ISubscriptionRegistry, ISubscriptionTag>>();
        }

        public IAmqpSubscriptionBuilder RouteTo<T>(string exchangeName, string routingKey)
        {
            this.router.Register<T>(m => exchangeName, m => routingKey);
            this.subscriptions.Add((connectionFactory, builder, bus) => bus.Subscribe<T>(new AmqpSender(connectionFactory, builder)));

            return this;
        }

        public IAmqpSubscriptionBuilder WithConnectionFactory(IAmqpConnectionFactory connectionFactory)
        {
            this.ConnectionFactory = connectionFactory;

            return this;
        }

        public IAmqpSubscriptionBuilder WithSerializer(string contentType, IAmqpSerializer serializer)
        {
            this.Serializers.Add(contentType, serializer);

            return this;
        }

        public ISubscriptionTag Build(ISubscriptionRegistry registry)
        {
            var serializerFactory = new AmqpSerializerFactory(this.Serializers);
            var amqpMessageBuilder = new AmqpMessageBuilder(serializerFactory, propertyBuilder, router);

            var subscriptionTags =
                this.subscriptions
                    .Select(createSubsriptionTag => createSubsriptionTag(this.ConnectionFactory, amqpMessageBuilder, registry))
                    .ToList();

            return new CompositeSubscriptionTag(Guid.NewGuid().ToString(), subscriptionTags);
        }
    }
}
