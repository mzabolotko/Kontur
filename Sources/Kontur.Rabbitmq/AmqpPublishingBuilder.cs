using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.Rabbitmq
{
    public class AmqpPublishingBuilder : IAmqpPublishingBuilder
    {
        private readonly AmqpPropertyBuilder propertyBuilder;
        private readonly List<Func<MessageBuilder, IAmqpConnectionFactory, IPublisherRegistry, IPublishingTag>> publishers;

        private IAmqpConnectionFactory connectionFactory;
        private IAmqpDeserializerFactory deserializerFactory;
        private MessageBuilder messageBuilder;

        public AmqpPublishingBuilder()
        {
            this.propertyBuilder = new AmqpPropertyBuilder();
            this.connectionFactory = new AmqpConnectionFactory(new Uri("amqp://"));
            this.publishers = new List<Func<MessageBuilder, IAmqpConnectionFactory, IPublisherRegistry, IPublishingTag>>();
        }

        public IAmqpDeserializerFactory DeserializerFactory => this.deserializerFactory;

        public IAmqpConnectionFactory ConnectionFactory => this.connectionFactory;

        public IAmqpPublishingBuilder ReactOn<T>(string queue)
        {
            this.publishers.Add(
                (messageBuilder, connectionFactory, registry) =>
                    registry.RegisterPublisher<T>(new AmqpBasicConsumer<T>(
                        this.connectionFactory,
                        this.propertyBuilder,
                        messageBuilder,
                        false,
                        queue)));

            return this;
        }

        public IAmqpPublishingBuilder WithDeserializerFactory(IAmqpDeserializerFactory deserializerFactory)
        {
            this.deserializerFactory = deserializerFactory;
            this.messageBuilder = new MessageBuilder(this.deserializerFactory, this.propertyBuilder);

            return this;
        }

        public IAmqpPublishingBuilder WithConnectionFactory(IAmqpConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;

            return this;
        }

        public IPublishingTag Build(IPublisherRegistry registry)
        {
            List<IPublishingTag> tags = 
                this.publishers
                    .Select(createPublisher => createPublisher(this.messageBuilder, this.connectionFactory, registry))
                    .ToList();

            IPublishingTag tag = new CompositePublishingTag(Guid.NewGuid().ToString(), tags);

            return tag;
        }
    }
}
