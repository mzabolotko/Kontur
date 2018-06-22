using FakeItEasy;
using FluentAssertions;
using Kontur.Rabbitmq.Tests.Plumbing;
using NUnit.Framework;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    internal class AmqpSubscriptionBuilderFixture
    {
        [Test]
        public void SetConnectionFactory()
        {
            var connectionFactory = A.Fake<IAmqpConnectionFactory>();
            var sut = new AmqpSubscriptionBuilder();

            sut.WithConnectionFactory(connectionFactory);

            sut.ConnectionFactory.Should().Be(connectionFactory, because: "Connection factory should be set.");
        }

        [Test]
        public void BuildWithoutSubscribers()
        {
            var registry = A.Fake<ISubscriptionRegistry>();
            var sut = new AmqpSubscriptionBuilder();

            ISubscriptionTag publishingTag = sut.Build(registry);

            A.CallTo(registry)
                .Where(call => call.Method.Name == "Subscribe")
                .MustNotHaveHappened();
        }

        [Test]
        public void BuildWithSubscribers()
        {
            var registry = A.Fake<ISubscriptionRegistry>();
            var sut = new AmqpSubscriptionBuilder();
            sut.RouteTo<string>("test1", "test1");
            sut.RouteTo<string>("test2", "test2");

            ISubscriptionTag publishingTag = sut.Build(registry);

            A.CallTo(() => registry.Subscribe<string>(A<ISubscriber>.Ignored)).MustHaveHappenedTwiceExactly();
        }

        [Test]
        public void BuildWithSerializer()
        {
            var serializer = A.Fake<IAmqpSerializer>();
            var sut = new AmqpSubscriptionBuilder();

            sut.WithSerializer("contentType", serializer);

            sut.Serializers.Should().HaveCount(2, because: "one more serializer should be added to the collection");
        }
    }
}
