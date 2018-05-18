using System.Collections.Generic;

namespace Kontur.Rabbitmq
{
    internal class MessageBuilder : IMessageBuilder
    {
        private readonly IAmqpDeserializerFactory deserializerFactory;
        private readonly IAmqpPropertyBuilder propertyBuilder;

        public MessageBuilder(IAmqpDeserializerFactory deserializerFactory, IAmqpPropertyBuilder propertyBuilder)
        {
            this.deserializerFactory = deserializerFactory;
            this.propertyBuilder = propertyBuilder;
        }

        public IMessage Build<T>(AmqpMessage amqpMessage)
        {
            IAmqpDeserializer amqpDeserializer = this.deserializerFactory.CreateDeserializer(amqpMessage);
            T payload = amqpDeserializer.Deserialize<T>(amqpMessage);
            IDictionary<string, string> headers = this.propertyBuilder.BuildHeadersFromProperties(amqpMessage.Properties);
            return new Message<T>(payload, new Dictionary<string, string>(headers));
        }
    }
}
