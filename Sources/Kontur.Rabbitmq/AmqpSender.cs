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

            var amqpBuilderBlock = new TransformBlock<IMessage, AmqpMessageResult>(
                (Func<IMessage, AmqpMessageResult>)((IMessage message) => {
                    try
                    {
                        return new AmqpMessageResult(amqpMessageBuilder.Serialize(message));
                    }
                    catch (Exception ex)
                    {
                        return new AmqpMessageResult(ExceptionDispatchInfo.Capture(ex));
                    }}));

            var amqpSenderBlock = new ActionBlock<AmqpMessageResult>(
                    (Action<AmqpMessageResult>)this.Send);

            this.link = source.LinkTo(amqpBuilderBlock);
            amqpBuilderBlock.LinkTo(amqpSenderBlock);

            return new SubscribingTag(Guid.NewGuid().ToString(), this.CancelSending);
        }

        private void Send(AmqpMessageResult result)
        {
            try
            {
                if (!result.Success)
                {
                    return;
                }

                AmqpMessage message = result.Value;

                IBasicProperties basicProperties = this.model.CreateBasicProperties();
                message.Properties.CopyTo(basicProperties);

                this.model.BasicPublish(
                    message.ExchangeName,
                    message.RoutingKey,
                    false,
                    basicProperties,
                    message.Payload);
            }
            catch (Exception)
            {
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
