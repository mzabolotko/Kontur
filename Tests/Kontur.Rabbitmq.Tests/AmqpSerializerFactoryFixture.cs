using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    class AmqpSerializerFactoryFixture
    {
        [Test]
        public void CanCreateAmqpSerializerWithDefaultSerializerIfNotAnyRegistered()
        {
            IMessage message = A.Fake<IMessage>();
            IAmqpSerializer defaultSerializer = A.Fake<IAmqpSerializer>();
            var sut = new AmqpSerializerFactory("plain/text", defaultSerializer);

            IAmqpSerializer serializer = sut.CreateSerializer(message);

            serializer.Should().NotBeNull(because: "factory should return default serializer");
        }
    }
}
