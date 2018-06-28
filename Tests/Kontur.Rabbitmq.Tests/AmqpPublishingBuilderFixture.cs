using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    internal class AmqpPublishingBuilderFixture
    {
        [Test]
        public void CanSetDeserializer()
        {
            var serializer = A.Fake<IAmqpSerializer>();
            var sut = new AmqpPublishingBuilder();

            sut.WithDeserializer("contentType", serializer);

            sut.Serializers.Should().HaveCount(2, because: "one more serializer should be added to the collection");
        }

        [Test]
        public void CanSetConnectionFactory()
        {
            var connectionFactory = A.Fake<IAmqpConnectionFactory>();
            var sut = new AmqpPublishingBuilder();

            sut.WithConnectionFactory(connectionFactory);

            sut.ConnectionFactory.Should().Be(connectionFactory, because: "Connection factory should be set.");
        }

        [Test]
        public void CanBuildWithoutPublishers()
        {
            var registry = A.Fake<IPublisherRegistry>();
            var sut = new AmqpPublishingBuilder();

            IPublishingTag publishingTag = sut.Build(registry);

            A.CallTo(() => registry.RegisterPublisher(A<IPublisher>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void CanBuildWithPublishers()
        {
            var registry = A.Fake<IPublisherRegistry>();
            var sut = new AmqpPublishingBuilder();

            sut.ReactOn<string>("test1");
            sut.ReactOn<string>("test2");
            IPublishingTag publishingTag = sut.Build(registry);

            A.CallTo(() => registry.RegisterPublisher(A<IPublisher>.Ignored)).MustHaveHappenedTwiceExactly();
        }
    }
}
