namespace Kontur.Rabbitmq
{
    public interface IAmqpSubscriptionBuilder
    {
        IAmqpSubscriptionBuilder RouteTo<T>(string exchangeName, string routingKey);

        IAmqpSubscriptionBuilder WithConnectionFactory(IAmqpConnectionFactory connectionFactory);

        IAmqpSubscriptionBuilder WithSerializer(string contentType, IAmqpSerializer serializer);

        IAmqpConnectionFactory ConnectionFactory { get; }
    }
}
