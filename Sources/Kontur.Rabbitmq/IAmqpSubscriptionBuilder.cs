namespace Kontur.Rabbitmq
{
    public interface IAmqpSubscriptionBuilder
    {
        IAmqpSubscriptionBuilder RouteTo<T>(string exchangeName, string routingKey);
    }
}
