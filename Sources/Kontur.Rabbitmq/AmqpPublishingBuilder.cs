using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.Rabbitmq
{
    public class AmqpPublishingBuilder : IAmqpPublishingBuilder
    {
        private readonly IAmqpRouter router;
        private readonly IAmqpPropertyBuilder propertyBuilder;
        private readonly List<Func<IAmqpMessageBuilder, IAmqpConnectionFactory, IPublisherRegistry, IPublishingTag>> publishers;

        public IAmqpConnectionFactory ConnectionFactory { get; private set; }
        public IDictionary<string, IAmqpSerializer> Serializers { get; }

        public AmqpPublishingBuilder()
        {
            this.Serializers = new Dictionary<string, IAmqpSerializer>
            {
                { "plain/text", new SimpleSerializer() }
            };
            this.router = new AmqpRouter();
            this.propertyBuilder = new AmqpPropertyBuilder();
            this.ConnectionFactory = new AsyncAmqpConnectionFactory(new Uri("amqp://"));
            this.publishers = new List<Func<IAmqpMessageBuilder, IAmqpConnectionFactory, IPublisherRegistry, IPublishingTag>>();
        }

        public IAmqpPublishingBuilder ReactOn<T>(string queue) where T : class
        {
            this.publishers.Add(
                (messageBuilder, connectionFactory, registry) =>
                    registry.RegisterPublisher<T>(
                        new AsyncAmqpBasicConsumer<T>(
                        this.ConnectionFactory,
                        this.propertyBuilder,
                        messageBuilder,
                        false,
                        queue)));

            return this;
        }

        public IAmqpPublishingBuilder WithConnectionFactory(IAmqpConnectionFactory connectionFactory)
        {
            this.ConnectionFactory = connectionFactory;

            return this;
        }

        public IAmqpPublishingBuilder WithDeserializer(string contentType, IAmqpSerializer serializer)
        {
            this.Serializers.Add(contentType, serializer);

            return this;
        }

        public IPublishingTag Build(IPublisherRegistry registry)
        {
            var serializerFactory = new AmqpSerializerFactory(this.Serializers);
            var messageBuilder = new AmqpMessageBuilder(serializerFactory, this.propertyBuilder, this.router);

            List<IPublishingTag> tags =
                this.publishers
                    .Select(createPublisher => createPublisher(messageBuilder, this.ConnectionFactory, registry))
                    .ToList();

            return new CompositePublishingTag(Guid.NewGuid().ToString(), tags);
        }
    }
}
