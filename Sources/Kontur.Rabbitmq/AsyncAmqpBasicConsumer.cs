using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur.Rabbitmq
{
    class AsyncAmqpBasicConsumer<T> : IPublisher where T : class 
    {
        private readonly IAmqpConnectionFactory connectionFactory;
        private readonly IAmqpPropertyBuilder amqpPropertyBuilder;
        private readonly IAmqpMessageBuilder amqpMessageBuilder;
        private readonly bool continueOnCapturedContext;
        private readonly string queue;
        private IModel channel;
        private IConnection connection;
        private IDisposable link;
        private TransformBlock<AmqpMessage, IMessage> convertBlock;

        public AsyncAmqpBasicConsumer(
            IAmqpConnectionFactory connectionFactory,
            IAmqpPropertyBuilder amqpPropertyBuilder,
            IAmqpMessageBuilder amqpMessageBuilder,
            bool continueOnCapturedContext,
            string queue)
        {
            this.connectionFactory = connectionFactory;
            this.amqpPropertyBuilder = amqpPropertyBuilder;
            this.amqpMessageBuilder = amqpMessageBuilder;
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
            if (this.connection != null || this.channel != null || this.convertBlock != null)
            {
                throw new InvalidOperationException("You could not link it more than once.");
            }

            this.connection = this.connectionFactory.CreateConnection();
            this.channel = this.connection.CreateModel();

            this.convertBlock = new TransformBlock<AmqpMessage, IMessage>(
                (Func<AmqpMessage, IMessage>) this.amqpMessageBuilder.Deserialize<T>);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += OnReceived;
            string consumerTag = channel.BasicConsume(this.queue, false, consumer);

            this.link = this.convertBlock.LinkTo(target);

            return new PublishingTag(consumerTag, CancelConsuming);
        }

        public async Task OnReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            var result = await this.convertBlock.SendAsync(
                new AmqpMessage(
                    amqpPropertyBuilder.BuildPropertiesFromProperties(eventArgs.BasicProperties),
                    eventArgs.Exchange,
                    eventArgs.RoutingKey,
                    eventArgs.Body)
            ).ConfigureAwait(this.continueOnCapturedContext);

            this.channel.BasicAck(eventArgs.DeliveryTag, false);
        }
    }
}
