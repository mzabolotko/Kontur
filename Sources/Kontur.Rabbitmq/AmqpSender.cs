using RabbitMQ.Client;

namespace Kontur.Rabbitmq
{
    internal class AmqpSender
    {
        private readonly IModel model;

        public AmqpSender(IModel model)
        {
            this.model = model;
        }

        public void Send(AmqpMessage message)
        {
            IBasicProperties basicProperties = this.model.CreateBasicProperties();
            message.Properties.CopyTo(basicProperties);

            this.model.BasicPublish(
                message.ExchangeName, 
                message.RoutingKey, 
                basicProperties, 
                message.Payload);
        }
    }

}
