using RabbitMQ.Client;
using System;

namespace Kontur.Rabbitmq
{
    public class AmqpSubscriptionTag : ISubscriptionTag
    {
        private readonly ISubscriptionTag subscriptionTag;
        private readonly IConnection connection;
        private readonly IModel model;

        public AmqpSubscriptionTag(ISubscriptionTag subscriptionTag, IConnection connection, IModel model)
        {
            this.subscriptionTag = subscriptionTag;
            this.connection = connection;
            this.model = model;
        }

        public Guid Id => this.subscriptionTag.Id;

        public void Dispose()
        {
            this.subscriptionTag.Dispose();
            this.model.Dispose();
            this.connection.Dispose();
        }
    }
}
