using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    internal class AmqpMessageFixture
    {
        [Test]
        public void CanCreateAmqpMessage()
        {
            IAmqpProperties properties = A.Fake<IAmqpProperties>();
            byte[] payload = new byte[0];
            const string ExchangeName = "_exchangeName_";
            const string RoutingKey = "_routingKey_";

            var sut = new AmqpMessage(properties, ExchangeName, RoutingKey, payload);

            sut.ExchangeName.Should().Be(ExchangeName, because: "constructor should be set it.");
            sut.RoutingKey.Should().Be(RoutingKey, because: "constructor should be set it.");
            sut.Payload.Should().BeEquivalentTo(payload, because: "constructor should be set it.");
            sut.Properties.Should().Be(properties, because: "constructor should be set it.");
        }
    }
}
