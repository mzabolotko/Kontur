namespace Kontur.Rabbitmq
{
    public interface IAmqpPublishingBuilder
    {
        IAmqpPublishingBuilder WithDeserializer(string contentType, IAmqpSerializer serializer);

        IAmqpPublishingBuilder WithConnectionFactory(IAmqpConnectionFactory connectionFactory);

        IAmqpPublishingBuilder ReactOn<T>(string queue) where T : class;

        IAmqpConnectionFactory ConnectionFactory { get; }
    }
}
