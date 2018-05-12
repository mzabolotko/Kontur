using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    internal class AmqpPublishingBuilderFixture
    {
        [Test]
        public void CanSetDeserializerFactory()
        {
            IAmqpDeserializerFactory deserializerFactory = A.Fake<IAmqpDeserializerFactory>();

            var sut = new AmqpPublishingBuilder();

            sut.WithDeserializerFactory(deserializerFactory);

            sut.DeserializerFactory.Should().Be(deserializerFactory, because: "Deserializer factory should be set.");
        }

        [Test]
        public void CanSetConnectionFactory()
        {
            IAmqpConnectionFactory connectionFactory = A.Fake<IAmqpConnectionFactory>();

            var sut = new AmqpPublishingBuilder();

            sut.WithConnectionFactory(connectionFactory);

            sut.ConnectionFactory.Should().Be(connectionFactory, because: "Connection factory should be set.");
        }

        [Test]
        public void CanBuildWithoutPublishers()
        {
            IPublisherRegistry registry = A.Fake<IPublisherRegistry>();

            var sut = new AmqpPublishingBuilder();
            IPublishingTag publishingTag = sut.Build(registry);

            A.CallTo(() => registry.RegisterPublisher<int>(A<IPublisher>.Ignored)).MustNotHaveHappened();            
        }

        [Test]
        public void CanBuildWithPublishers()
        {
            IPublisherRegistry registry = A.Fake<IPublisherRegistry>();

            var sut = new AmqpPublishingBuilder();
            sut.ReactOn<string>("test1");
            sut.ReactOn<string>("test2");
            IPublishingTag publishingTag = sut.Build(registry);

            A.CallTo(() => registry.RegisterPublisher<string>(A<IPublisher>.Ignored)).MustHaveHappenedTwiceExactly();
        }
    }
}
