
namespace Kontur.Rabbitmq
{
    internal class AmqpDelivery
    {
        public AmqpDelivery(AmqpMessage amqpMessage, ulong deliveryTag)
        {
            this.Message = amqpMessage;
            this.DeliveryTag = deliveryTag;
        }

        public AmqpMessage Message { private set; get; }
        public ulong DeliveryTag { private set; get; }
    }
}
