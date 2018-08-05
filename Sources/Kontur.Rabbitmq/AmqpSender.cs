using System;
using System.Threading.Tasks.Dataflow;
using RabbitMQ.Client;

namespace Kontur.Rabbitmq
{
    internal class AmqpSender : ISubscriber
    {
        private readonly IAmqpConnectionFactory connectionFactory;
        private readonly IAmqpMessageBuilder amqpMessageBuilder;
        private IDisposable link;
        private IModel model;
        private IConnection connection;

        public AmqpSender(IAmqpConnectionFactory connectionFactory, IAmqpMessageBuilder amqpMessageBuilder)
        {
            this.connectionFactory = connectionFactory;
            this.amqpMessageBuilder = amqpMessageBuilder;
        }

        public ISubscriptionTag SubscribeTo(ISourceBlock<IMessage> source)
        {
            this.connection = this.connectionFactory.CreateConnection();
            this.model = this.connection.CreateModel();

            var amqpBuilderBlock = new TransformBlock<IMessage, AmqpMessage>(
                    (Func<IMessage, AmqpMessage>)amqpMessageBuilder.Serialize);

            var amqpSenderBlock = new ActionBlock<AmqpMessage>(
                    (Action<AmqpMessage>)this.Send);

            this.link = source.LinkTo(amqpBuilderBlock);
            amqpBuilderBlock.LinkTo(amqpSenderBlock);

            return new SubscribingTag(Guid.NewGuid().ToString(), this.CancelSending);
        }

        private void Send(AmqpMessage message)
        {
            IBasicProperties basicProperties = this.model.CreateBasicProperties();
            message.Properties.CopyTo(basicProperties);

            this.model.BasicPublish(
                message.ExchangeName,
                message.RoutingKey,
                basicProperties,
                message.Payload);
        }

        private void CancelSending()
        {
            this.link.Dispose();
            this.model.Close();
            this.connection.Close();
        }
    }
}
