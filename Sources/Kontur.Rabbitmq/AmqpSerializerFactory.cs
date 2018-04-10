namespace Kontur.Rabbitmq
{
    internal class AmqpSerializerFactory : IAmqpSerializerFactory
    {
        private readonly string defaultContentType;
        private readonly IAmqpSerializer defaultSerializer;

        public AmqpSerializerFactory(string defaultContentType, IAmqpSerializer defaultSerializer)
        {
            this.defaultContentType = defaultContentType;
            this.defaultSerializer = defaultSerializer;
        }

        public IAmqpSerializer CreateSerializer(IMessage message)
        {
            return defaultSerializer;
        }
    }

}
