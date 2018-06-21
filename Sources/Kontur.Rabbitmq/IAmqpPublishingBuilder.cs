namespace Kontur.Rabbitmq
{
    public interface IAmqpPublishingBuilder
    {
        IAmqpPublishingBuilder WithDeserializerFactory(IAmqpDeserializerFactory deserializerFactory);
        IAmqpPublishingBuilder WithConnectionFactory(IAmqpConnectionFactory connectionFactory);
        IAmqpPublishingBuilder ReactOn<T>(string queue);

        IAmqpDeserializerFactory DeserializerFactory { get; }
        IAmqpConnectionFactory ConnectionFactory { get; }
    }
}
