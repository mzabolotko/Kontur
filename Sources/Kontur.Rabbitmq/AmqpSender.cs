using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks.Dataflow;
using RabbitMQ.Client;

using AmqpMessageResult =
    Kontur.Result<Kontur.Rabbitmq.AmqpMessage, System.Runtime.ExceptionServices.ExceptionDispatchInfo>;

namespace Kontur.Rabbitmq
{
    internal class AmqpSender : ISubscriber
    {
        private readonly IAmqpConnectionFactory connectionFactory;
        private readonly IAmqpMessageBuilder amqpMessageBuilder;
        private readonly ILogServiceProvider logServiceProvider;
        private readonly ILogService logService;
        private IDisposable link;
        private IModel model;
        private IConnection connection;

        public AmqpSender(IAmqpConnectionFactory connectionFactory, IAmqpMessageBuilder amqpMessageBuilder, ILogServiceProvider logServiceProvider = null)
        {
            this.connectionFactory = connectionFactory;
            this.amqpMessageBuilder = amqpMessageBuilder;
            this.logServiceProvider = logServiceProvider ?? new NullLogServiceProvider();
            this.logService = this.logServiceProvider.GetLogServiceOf(typeof(AmqpSender));
        }

        public ISubscriptionTag SubscribeTo(ISourceBlock<IMessage> source)
        {
            this.logService.Debug("Subscribing");
            this.connection = this.connectionFactory.CreateConnection();
            this.model = this.connection.CreateModel();

            var amqpBuilderBlock = new TransformBlock<IMessage, AmqpMessageResult>(
                (Func<IMessage, AmqpMessageResult>)this.Transform);

            var amqpSenderBlock = new ActionBlock<AmqpMessageResult>(
                    (Action<AmqpMessageResult>)this.Send);

            this.link = source.LinkTo(amqpBuilderBlock);
            amqpBuilderBlock.LinkTo(amqpSenderBlock);

            return new SubscribingTag(Guid.NewGuid().ToString(), this.CancelSending);
        }

        public AmqpMessageResult Transform(IMessage message)
        {
            try
            {
                this.logService.Debug("Building message to send.");
                return new AmqpMessageResult(amqpMessageBuilder.Serialize(message));
            }
            catch (Exception ex)
            {
                this.logService.Warn(ex, "Building message was failed.");
                return new AmqpMessageResult(ExceptionDispatchInfo.Capture(ex));
            }
        }

        public void Send(AmqpMessageResult result)
        {
            try
            {
                this.logService.Debug("Sending the message.");
                if (!result.Success)
                {
                    return;
                }

                AmqpMessage message = result.Value;

                IBasicProperties basicProperties = this.model.CreateBasicProperties();
                message.Properties.CopyTo(basicProperties);

                this.logService.Trace("Sending the message to the {0} exchange with {1} routekey.", message.ExchangeName, message.RoutingKey);
                this.model.BasicPublish(
                    message.ExchangeName,
                    message.RoutingKey,
                    false,
                    basicProperties,
                    message.Payload);
            }
            catch (Exception ex)
            {
                this.logService.Warn(ex, "Sending was failed.");
            }
        }

        private void CancelSending()
        {
            this.link.Dispose();
            this.model.Close();
            this.connection.Close();
        }
    }
}
