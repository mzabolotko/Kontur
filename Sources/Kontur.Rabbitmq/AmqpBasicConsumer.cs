using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks.Dataflow;

namespace Kontur.Rabbitmq
{
    class AmqpBasicConsumer<T> : IPublisher
    {
        private readonly IAmqpConnectionFactory connectionFactory;
        private readonly IAmqpPropertyBuilder amqpPropertyBuilder;
        private readonly IMessageBuilder messageBuilder;
        private readonly bool continueOnCapturedContext;
        private readonly string queue;
        private IModel channel;
        private IConnection connection;
        private IDisposable link;

        public AmqpBasicConsumer(
            IAmqpConnectionFactory connectionFactory, 
            IAmqpPropertyBuilder amqpPropertyBuilder, 
            IMessageBuilder messageBuilder, 
            bool continueOnCapturedContext, 
            string queue)
        {
            this.connectionFactory = connectionFactory;
            this.amqpPropertyBuilder = amqpPropertyBuilder;
            this.messageBuilder = messageBuilder;
            this.continueOnCapturedContext = continueOnCapturedContext;
            this.queue = queue;
        }

        private void CancelConsuming(string consumerTag)
        {
            this.link.Dispose();
            this.channel.BasicCancel(consumerTag);
            this.channel.Close();
            this.connection.Close();
        }

        public IPublishingTag LinkTo(ITargetBlock<IMessage> target)
        {
            this.connection = this.connectionFactory.CreateConnection();
            this.channel = this.connection.CreateModel();

            var transformBlock = new TransformBlock<AmqpMessage, IMessage>(
                (Func<AmqpMessage, IMessage>) this.messageBuilder.Build<T>);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (ch, ea) =>
            {
                var result = await transformBlock.SendAsync(
                    new AmqpMessage(
                        amqpPropertyBuilder.BuildPropertiesFromProperties(ea.BasicProperties),
                        ea.Exchange,
                        ea.RoutingKey,
                        ea.Body)
                ).ConfigureAwait(this.continueOnCapturedContext);

                channel.BasicAck(ea.DeliveryTag, false);
            };
            string consumerTag = channel.BasicConsume(this.queue, false, consumer);

            this.link = transformBlock.LinkTo(target);

            return new PublishingTag(consumerTag, CancelConsuming);
        }
    }
}
