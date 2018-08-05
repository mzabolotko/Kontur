using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    internal class AmqpSubscriptionBuilderFixture
    {
        [Test]
        public void CanSetConnectionFactory()
        {
            IAmqpConnectionFactory connectionFactory = A.Fake<IAmqpConnectionFactory>();

            var sut = new AmqpSubscriptionBuilder();

            sut.WithConnectionFactory(connectionFactory);

            sut.ConnectionFactory.Should().Be(connectionFactory, because: "Connection factory should be set.");
        }

        [Test]
        public void CanBuildWithoutSubscribers()
        {
            ISubscriptionRegistry registry = A.Fake<ISubscriptionRegistry>();

            var sut = new AmqpSubscriptionBuilder();
            ISubscriptionTag publishingTag = sut.Build(registry);

            A.CallTo(registry)
                .Where(call => call.Method.Name == "Subscribe")
                .MustNotHaveHappened();
        }

        [Test]
        public void CanBuildWithSubscribers()
        {
            ISubscriptionRegistry registry = A.Fake<ISubscriptionRegistry>();

            var sut = new AmqpSubscriptionBuilder();
            sut.RouteTo<string>("test1", "test1");
            sut.RouteTo<string>("test2", "test2");
            ISubscriptionTag publishingTag = sut.Build(registry);

            A.CallTo(() => registry.Subscribe<string>(A<ISubscriber>.Ignored, A<int>.Ignored)).MustHaveHappenedTwiceExactly();
        }
    }
}
