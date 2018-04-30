using System;

namespace Kontur.Rabbitmq
{
    public static class BusExtensions
    {
        public static ISubscriptionTag ToRabbitMq(this Bus bus, Func<IAmqpSubscriptionBuilder, IAmqpSubscriptionBuilder> build)
        {
            AmqpSubscriptionBuilder builder = new AmqpSubscriptionBuilder();
            builder = (AmqpSubscriptionBuilder) build(builder);
            return builder.Build(bus);
        }

        public static IPublishingTag FromRabbitMq(this Bus bus, Func<IAmqpPublishingBuilder, IAmqpPublishingBuilder> build)
        {
            AmqpPublishingBuilder builder = new AmqpPublishingBuilder();
            builder = (AmqpPublishingBuilder) build(builder);
            return builder.Build(bus);
        }
    }
}
