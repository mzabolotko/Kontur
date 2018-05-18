namespace Kontur.Rabbitmq
{
    public interface IAmqpSubscriptionBuilder
    {
        IAmqpSubscriptionBuilder RouteTo<T>(string exchangeName, string routingKey);
        IAmqpSubscriptionBuilder WithConnectionFactory(IAmqpConnectionFactory connectionFactory);

        IAmqpConnectionFactory ConnectionFactory { get; }
    }
}
