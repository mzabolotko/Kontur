using System.Threading.Tasks.Dataflow;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Tests
{
    [TestFixture]
    public class OutboxFixture
    {
        [Test(Description = "Can create subscriber queue")]
        public void CanCreateSubscriberQueue()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageBuffer queue = A.Fake<IMessageBuffer>();

            A.CallTo(() => messageBufferFactory.Create(A<int?>.Ignored)).Returns(queue);

            var sut = new Outbox(messageBufferFactory, messageActionFactory, logServiceProvider);

            IMessageBuffer result = sut.CreateSubscriberQueue<string>();

            result.Should().Be(queue, because: "The subscriber queue was created by MessageBufferFactory");
        }

        [Test(Description = "Can subscribe the subscriber")]
        public void CanSubscribeSubscriber()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageBuffer queue = A.Fake<IMessageBuffer>();
            ISubscriber subscriber = A.Fake<ISubscriber>();
            ISubscriptionTag subscriptionTag = A.Fake<ISubscriptionTag>();

            A.CallTo(() => subscriber.SubscribeTo(A<ISourceBlock<IMessage>>.Ignored)).Returns(subscriptionTag);

            var sut = new Outbox(messageBufferFactory, messageActionFactory, logServiceProvider);

            ISubscriptionTag result = sut.Subscribe<string>(queue, subscriber);

            result.Should().Be(subscriptionTag, because: "The subscription tag was created by a subscriber.");
        }
    }
}
