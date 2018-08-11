using System.Runtime.ExceptionServices;

namespace Kontur.Rabbitmq
{
    internal class AmqpDeliveryError
    {
        public AmqpDeliveryError(AmqpDelivery delivery, ExceptionDispatchInfo exception)
        {
            this.Delivery = delivery;
            this.Exception = exception;
        }

        public AmqpDelivery Delivery { private set; get; }
        public ExceptionDispatchInfo Exception { private set; get; }
    }
}
