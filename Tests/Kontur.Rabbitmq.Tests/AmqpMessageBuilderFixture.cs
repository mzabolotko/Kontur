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
            IAmqpPropertyBuilder propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            IAmqpRouter router = A.Fake<IAmqpRouter>();
            IAmqpSerializerFactory serializerFactory = A.Fake<IAmqpSerializerFactory>();

            IMessage message = A.Fake<IMessage>();
            A.CallTo(() => message.Headers).Returns(new Dictionary<string, string>() { });
            A.CallTo(() => message.RouteKey).Returns(typeof(object));

            var sut = new AmqpMessageBuilder(
                                        propertyBuilder,
                                        router,
                                        serializerFactory);

            AmqpMessage amqpMessage = sut.Build(message);

            amqpMessage.Properties.Should().NotBeNull(because : "Source message contains headers");
        }

        [Test(Description = "Can build an amqp message with exchange name")]
        public void CanBuildAmqpMessageWithExchangeName()
        {
            IAmqpPropertyBuilder propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            IAmqpRouter router = A.Fake<IAmqpRouter>();
            const string Command = "command.do.something";
            A.CallTo(() => router.GetExchange(A<IMessage>.Ignored)).Returns(Command);
            IAmqpSerializerFactory serializerFactory = A.Fake<IAmqpSerializerFactory>();

            IMessage message = A.Fake<IMessage>();
            A.CallTo(() => message.Headers).Returns(new Dictionary<string, string>() { });
            A.CallTo(() => message.RouteKey).Returns(typeof(object));

            var sut = new AmqpMessageBuilder(
                                        propertyBuilder,
                                        router,
                                        serializerFactory);

            AmqpMessage amqpMessage = sut.Build(message);

            amqpMessage.ExchangeName.Should().NotBeNull(because: "Source message contains the payload type");
            amqpMessage.ExchangeName.Should().Be(Command, because: "The router evaluates exchange name");
        }

        [Test(Description = "Can build an amqp message with routing key")]
        public void CanBuildAmqpMessageWithRoutingKey()
        {
            IAmqpPropertyBuilder propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            IAmqpRouter router = A.Fake<IAmqpRouter>();
            A.CallTo(() => router.GetRoutingKey(A<IMessage>.Ignored)).Returns("nothing");
            IAmqpSerializerFactory serializerFactory = A.Fake<IAmqpSerializerFactory>();

            IMessage message = A.Fake<IMessage>();
            A.CallTo(() => message.Headers).Returns(new Dictionary<string, string>() { });
            A.CallTo(() => message.RouteKey).Returns(typeof(object));

            var sut = new AmqpMessageBuilder(
                                        propertyBuilder,
                                        router,
                                        serializerFactory);

            AmqpMessage amqpMessage = sut.Build(message);

            amqpMessage.RoutingKey.Should().NotBeNull(because: "Router evaluates routing key");
            amqpMessage.RoutingKey.Should().Be("nothing", because: "Router evaluates routing key");
        }

        [Test(Description = "Can build an amqp message with payload")]
        public void CanBuildAmqpMessageWithPayload()
        {
            byte[] payload = new byte[] { 1, 2, 3 };
            IAmqpPropertyBuilder propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            IAmqpRouter router = A.Fake<IAmqpRouter>();
            IAmqpSerializerFactory serializerFactory = A.Fake<IAmqpSerializerFactory>();
            IAmqpSerializer serializer = A.Fake<IAmqpSerializer>();
            A.CallTo(() => serializerFactory.CreateSerializer(A<IMessage>.Ignored)).Returns(serializer);
            A.CallTo(() => serializer.Serialize(A<IMessage>.Ignored)).Returns(payload);

            IMessage message = A.Fake<IMessage>();
            A.CallTo(() => message.Headers).Returns(new Dictionary<string, string>() { });
            A.CallTo(() => message.RouteKey).Returns(typeof(object));

            var sut = new AmqpMessageBuilder(
                                        propertyBuilder,
                                        router,
                                        serializerFactory);

            AmqpMessage amqpMessage = sut.Build(message);

            amqpMessage.Payload.Should().NotBeNull(because: "Router evaluates payload");
            amqpMessage.Payload.Should().BeEquivalentTo(payload, because: "Router evaluates payload");
        }
    }
}
