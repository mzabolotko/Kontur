using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using MessageResult = Kontur.Result<Kontur.IMessage, Kontur.Rabbitmq.AmqpDeliveryError>;

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
        private IDisposable targetLink;
        private IDisposable unpackLink;
        private IDisposable handleErrorLink;
        private TransformBlock<AmqpDelivery, MessageResult> deserializeBlock;
        private TransformBlock<MessageResult, IMessage> unpackBlock;
        private ActionBlock<MessageResult> handleErrorBlock;

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

        public IPublishingTag LinkTo(ITargetBlock<IMessage> target)
        {
            if (this.connection != null || this.channel != null || this.deserializeBlock != null)
            {
                throw new InvalidOperationException("You could not link it more than once.");
            }

            this.connection = this.connectionFactory.CreateConnection();
            this.channel = this.connection.CreateModel();

            this.deserializeBlock = new TransformBlock<AmqpDelivery, MessageResult>(
                (Func<AmqpDelivery, MessageResult>)((delivery) =>
               {
                   try
                   {
                       return new MessageResult(
                           this.amqpMessageBuilder.Deserialize<T>(delivery.Message));
                   }
                   catch (Exception ex)
                   {
                       return new MessageResult(
                           new AmqpDeliveryError(delivery, ExceptionDispatchInfo.Capture(ex)));
                   }
               }));
            this.unpackBlock = new TransformBlock<MessageResult, IMessage>(
                (Func<MessageResult, IMessage>)(result => result.Value));
            this.handleErrorBlock = new ActionBlock<MessageResult>(
                ((Action<MessageResult>)(result =>
                    {
                        try
                        {
                            this.channel.BasicNack(result.Error.Delivery.DeliveryTag, false, false);
                        }
                        catch (Exception)
                        {
                        }
                    })));

            this.unpackLink = this.deserializeBlock.LinkTo(this.unpackBlock, result => result.Success);
            this.handleErrorLink = this.deserializeBlock.LinkTo(this.handleErrorBlock, result => !result.Success);
            this.targetLink = this.unpackBlock.LinkTo(target);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += OnReceived;
            string consumerTag = channel.BasicConsume(this.queue, false, consumer);

            return new PublishingTag(consumerTag, CancelConsuming);
        }

        public async Task OnReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            var result = await this.deserializeBlock.SendAsync(
                new AmqpDelivery(
                    new AmqpMessage(
                        amqpPropertyBuilder.BuildPropertiesFromProperties(eventArgs.BasicProperties),
                        eventArgs.Exchange,
                        eventArgs.RoutingKey,
                        eventArgs.Body),
                    eventArgs.DeliveryTag)
            ).ConfigureAwait(this.continueOnCapturedContext);

            this.channel.BasicAck(eventArgs.DeliveryTag, false);
        }

        private void CancelConsuming(string consumerTag)
        {
            this.targetLink.Dispose();
            this.unpackLink.Dispose();
            this.handleErrorLink.Dispose();
            this.channel.BasicCancel(consumerTag);
            this.channel.Close();
            this.connection.Close();
        }
    }
}
