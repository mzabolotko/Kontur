using System;
using System.Runtime.ExceptionServices;
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

            var amqpBuilderBlock = new TransformBlock<IMessage, Result<AmqpMessage>>(
                (Func<IMessage, Result<AmqpMessage>>)((IMessage message) => {
                    try
                    {
                        return new Result<AmqpMessage>(amqpMessageBuilder.Serialize(message));
                    }
                    catch (Exception ex)
                    {
                        return new Result<AmqpMessage>(ExceptionDispatchInfo.Capture(ex));
                    }}));

            var amqpSenderBlock = new ActionBlock<Result<AmqpMessage>>(
                    (Action<Result<AmqpMessage>>)this.Send);

            this.link = source.LinkTo(amqpBuilderBlock);
            amqpBuilderBlock.LinkTo(amqpSenderBlock);

            return new SubscribingTag(Guid.NewGuid().ToString(), this.CancelSending);
        }

        private void Send(Result<AmqpMessage> result)
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
