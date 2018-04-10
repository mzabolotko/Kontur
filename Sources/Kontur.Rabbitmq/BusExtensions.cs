using System;

namespace Kontur.Rabbitmq
{
    public static class BusExtensions
    {
        public static ISubscriptionTag WithRabbitMq<T>(this Bus bus, Func<IAmqpSubscriptionBuilder, IAmqpSubscriptionBuilder> build)
        {
            AmqpSubscriptionBuilder builder = new AmqpSubscriptionBuilder();
            builder = (AmqpSubscriptionBuilder) build(builder);
            return builder.Build<T>(bus);
        }
    }
}
