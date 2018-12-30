using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using MessageResult = Kontur.Result<Kontur.IMessage, Kontur.Rabbitmq.AmqpDeliveryError>;

namespace Kontur.Rabbitmq
{
    internal class AsyncAmqpBasicConsumer<T> : IPublisher where T : class
    {
        private readonly IAmqpConnectionFactory connectionFactory;
        private readonly IAmqpPropertyBuilder amqpPropertyBuilder;
        private readonly IAmqpMessageBuilder amqpMessageBuilder;
        private readonly bool continueOnCapturedContext;
        private readonly string queue;
        private readonly ILogServiceProvider logServiceProvder;
        private readonly ILogService logService;
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
            string queue,
            ILogServiceProvider logServiceProvder = null)
        {
            this.connectionFactory = connectionFactory;
            this.amqpPropertyBuilder = amqpPropertyBuilder;
            this.amqpMessageBuilder = amqpMessageBuilder;
            this.continueOnCapturedContext = continueOnCapturedContext;
            this.queue = queue;
            this.logServiceProvder = logServiceProvder ?? new NullLogServiceProvider();
            this.logService = this.logServiceProvder.GetLogServiceOf(typeof(AsyncAmqpBasicConsumer<T>));
        }

        public IPublishingTag LinkTo(ITargetBlock<IMessage> target)
        {
            this.logService.Debug("Linking to consume messages.");
            if (this.connection != null || this.channel != null || this.deserializeBlock != null)
            {
                throw new InvalidOperationException("You could not link it more than once.");
            }

            this.connection = this.connectionFactory.CreateConnection();
            this.channel = this.connection.CreateModel();

            this.deserializeBlock = new TransformBlock<AmqpDelivery, MessageResult>(delivery =>
            {
                try
                {
                    this.logService.Debug("Building message of {0} to consume.", typeof(T));
                    return new MessageResult(this.amqpMessageBuilder.Deserialize<T>(delivery.Message));
                }
                catch (Exception ex)
                {
                    this.logService.Warn(ex, "Buiding message of {0} to consume was failed.", typeof(T));
                    return new MessageResult(new AmqpDeliveryError(delivery, ExceptionDispatchInfo.Capture(ex)));
                }
            });
            this.unpackBlock = new TransformBlock<MessageResult, IMessage>(result => result.Value);
            this.handleErrorBlock = new ActionBlock<MessageResult>(result =>
            {
                try
                {
                    this.logService.Debug("Handling error message of {0}.", typeof(T));
                    this.channel.BasicNack(result.Error.Delivery.DeliveryTag, false, false);
                }
                catch (Exception ex)
                {
                    this.logService.Warn(ex, "Handling error message of {0} was failed.", typeof(T));
                }
            });

            this.unpackLink = this.deserializeBlock.LinkTo(this.unpackBlock, result => result.Success);
            this.handleErrorLink = this.deserializeBlock.LinkTo(this.handleErrorBlock, result => !result.Success);
            this.targetLink = this.unpackBlock.LinkTo(target);

            var consumer = new AsyncEventingBasicConsumer(this.channel);
            consumer.Received += this.OnReceived;
            string consumerTag = this.channel.BasicConsume(this.queue, false, consumer);

            return new PublishingTag(consumerTag, this.CancelConsuming);
        }

        public async Task OnReceived(object channel, BasicDeliverEventArgs eventArgs)
        {
            this.logService.Debug("Receiving message with '{0}' exchange and '{1}' routingkey.", eventArgs.Exchange, eventArgs.RoutingKey);
            var tcs = new TaskCompletionSource<bool>();

            var result = await this.deserializeBlock.SendAsync(
                new AmqpDelivery(
                    new AmqpMessage(
                        this.amqpPropertyBuilder.BuildPropertiesFromProperties(eventArgs.BasicProperties),
                        eventArgs.Exchange,
                        eventArgs.RoutingKey,
                        eventArgs.Body,
                        tcs),
                    eventArgs.DeliveryTag)
            ).ConfigureAwait(this.continueOnCapturedContext);

            this.channel.BasicAck(eventArgs.DeliveryTag, false);
        }

        private void CancelConsuming(string consumerTag)
        {
            this.logService.Debug("Canceling consuming messages.");
            this.targetLink.Dispose();
            this.unpackLink.Dispose();
            this.handleErrorLink.Dispose();
            this.channel.BasicCancel(consumerTag);
            this.channel.Close();
            this.connection.Close();
        }
    }
}
