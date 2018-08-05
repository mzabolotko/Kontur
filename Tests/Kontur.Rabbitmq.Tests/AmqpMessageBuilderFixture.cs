using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    public class AmqpMessageBuilderFixture
    {
        [Test(Description = "Can build an amqp message with basic properties")]
        public void CanBuildAmqpMessageWithBasicProperties()
        {
            var propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            var router = A.Fake<IAmqpRouter>();
            var serializerFactory = A.Fake<IAmqpSerializerFactory>();
            var message = GetMessage();

            var sut = new AmqpMessageBuilder(serializerFactory, propertyBuilder, router);

            AmqpMessage amqpMessage = sut.Serialize(message);

            amqpMessage.Properties.Should().NotBeNull(because : "Source message contains headers");
        }

        [Test(Description = "Can build an amqp message with exchange name")]
        public void CanBuildAmqpMessageWithExchangeName()
        {
            var propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            var router = A.Fake<IAmqpRouter>();
            const string Command = "command.do.something";
            A.CallTo(() => router.GetExchange(A<IMessage>.Ignored)).Returns(Command);
            var serializerFactory = A.Fake<IAmqpSerializerFactory>();
            var message = GetMessage();

            var sut = new AmqpMessageBuilder(serializerFactory, propertyBuilder, router);

            AmqpMessage amqpMessage = sut.Serialize(message);

            amqpMessage.ExchangeName.Should().NotBeNull(because: "Source message contains the payload type");
            amqpMessage.ExchangeName.Should().Be(Command, because: "The router evaluates exchange name");
        }

        [Test(Description = "Can build an amqp message with routing key")]
        public void CanBuildAmqpMessageWithRoutingKey()
        {
            var propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            var router = A.Fake<IAmqpRouter>();
            A.CallTo(() => router.GetRoutingKey(A<IMessage>.Ignored)).Returns("nothing");
            var serializerFactory = A.Fake<IAmqpSerializerFactory>();
            var message = GetMessage();

            var sut = new AmqpMessageBuilder(serializerFactory, propertyBuilder, router);

            AmqpMessage amqpMessage = sut.Serialize(message);

            amqpMessage.RoutingKey.Should().NotBeNull(because: "Router evaluates routing key");
            amqpMessage.RoutingKey.Should().Be("nothing", because: "Router evaluates routing key");
        }

        [Test(Description = "Can build an amqp message with payload")]
        public void CanBuildAmqpMessageWithPayload()
        {
            var payload = new byte[] { 1, 2, 3 };
            var propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            var router = A.Fake<IAmqpRouter>();
            var serializerFactory = A.Fake<IAmqpSerializerFactory>();
            var serializer = A.Fake<IAmqpSerializer>();
            A.CallTo(() => serializerFactory.CreateSerializer(A<IMessage>.Ignored)).Returns(serializer);
            A.CallTo(() => serializer.Serialize(A<IMessage>.Ignored)).Returns(payload);
            var message = GetMessage();

            var sut = new AmqpMessageBuilder(serializerFactory, propertyBuilder, router);

            AmqpMessage amqpMessage = sut.Serialize(message);

            amqpMessage.Payload.Should().NotBeNull(because: "Router evaluates payload");
            amqpMessage.Payload.Should().BeEquivalentTo(payload, because: "Router evaluates payload");
        }

        private static IMessage GetMessage()
        {
            var message = A.Fake<IMessage>();
            A.CallTo(() => message.Headers).Returns(new Dictionary<string, string>());
            A.CallTo(() => message.RouteKey).Returns(typeof(object));

            return message;
        }
    }
}
