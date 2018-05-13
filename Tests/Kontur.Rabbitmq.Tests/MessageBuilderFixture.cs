using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    internal class MessageBuilderFixture
    {
        [Test]
        public void CanBuildMessageFromAmqp()
        {
            const string payload = "hello world";
            IDictionary<string, string> headers = new Dictionary<string, string>
            {
                { "header1", "value1" }
            };

            IAmqpDeserializerFactory deserializerFactory = A.Fake<IAmqpDeserializerFactory>();
            IAmqpDeserializer deserializer = A.Fake<IAmqpDeserializer>();
            IAmqpPropertyBuilder propertyBuilder = A.Fake<IAmqpPropertyBuilder>();

            A.CallTo(() => deserializerFactory.CreateDeserializer(A<AmqpMessage>.Ignored)).Returns(deserializer);
            A.CallTo(() => deserializer.Deserialize<string>(A<AmqpMessage>.Ignored)).Returns(payload);
            A.CallTo(() => propertyBuilder.BuildHeadersFromProperties(A<IAmqpProperties>.Ignored)).Returns(headers);

            var sut = new MessageBuilder(deserializerFactory, propertyBuilder);
            IMessage message = sut.Build<string>(new AmqpMessage(null, "exchangeName", "routingKey", new byte[0]));

            message.RouteKey.Should().Be(typeof(System.String), because: "Routing key should be equal deserialized type.");
            message.Headers.Should().BeEquivalentTo(headers, because: "Headers builded from properties.");
            message.Payload.Should().BeEquivalentTo(payload, because: "Payload shoud be deserialized.");
        }
    }
}
