using System;
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
            ISubscriberFactory subscriberFactory = A.Fake<ISubscriberFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageBuffer queue = A.Fake<IMessageBuffer>();

            A.CallTo(() => messageBufferFactory.Create(A<int?>.Ignored)).Returns(queue);

            var sut = new Outbox(messageBufferFactory, subscriberFactory, logServiceProvider);

            IMessageBuffer result = sut.CreateSubscriberQueue<string>();

            result.Should().Be(queue, because: "The subscriber queue was created by MessageBufferFactory");
        }

        [Test(Description = "Can subscribe the subscriber")]
        public void CanSubscribeSubscriber()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            ISubscriberFactory subscriberFactory = A.Fake<ISubscriberFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageBuffer queue = A.Fake<IMessageBuffer>();
            ISubscriber subscriber = A.Fake<ISubscriber>();
            ISubscriptionTag subscriptionTag = A.Fake<ISubscriptionTag>();

            A.CallTo(() => subscriber.SubscribeTo(A<ISourceBlock<IMessage>>.Ignored)).Returns(subscriptionTag);

            var sut = new Outbox(messageBufferFactory, subscriberFactory, logServiceProvider);

            ISubscriptionTag result = sut.Subscribe<string>(queue, subscriber);

            result.Should().Be(subscriptionTag, because: "the subscription tag was created by a subscriber");
        }

        [Test(Description = "Can subscribe the action")]
        public void CanSubscribe()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            ISubscriberFactory subscriberFactory = A.Fake<ISubscriberFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageBuffer queue = A.Fake<IMessageBuffer>();
            ISubscriber subscriber = A.Fake<ISubscriber>();
            ISubscriptionTag subscriptionTag = A.Fake<ISubscriptionTag>();

            A.CallTo(() => subscriberFactory.Create(A<Action<Message<string>>>.Ignored)).Returns(subscriber);
            A.CallTo(() => subscriber.SubscribeTo(A<ISourceBlock<IMessage>>.Ignored)).Returns(subscriptionTag);

            var sut = new Outbox(messageBufferFactory, subscriberFactory, logServiceProvider);

            ISubscriptionTag result = sut.Subscribe<string>(queue, m => {});

            result.Should().Be(subscriptionTag, because: "the subscription tag was created by a subscriber");
        }

        [Test(Description = "Can subscribe the subscriber")]
        public void CanCheckSubscription()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            ISubscriberFactory subscriberFactory = A.Fake<ISubscriberFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageBuffer queue = A.Fake<IMessageBuffer>();
            ISubscriber subscriber = A.Fake<ISubscriber>();
            ISubscriptionTag subscriptionTag = A.Fake<ISubscriptionTag>();

            A.CallTo(() => subscriber.SubscribeTo(A<ISourceBlock<IMessage>>.Ignored)).Returns(subscriptionTag);

            var sut = new Outbox(messageBufferFactory, subscriberFactory, logServiceProvider);
            ISubscriptionTag tag = sut.Subscribe<string>(queue, subscriber);

            bool result = sut.IsSubscribed(tag);

            result.Should().BeTrue(because: "the subscriber was registered");
        }

        [Test(Description = "Can unsubscribe the unsubscribed subscriber")]
        public void CanUnsubscribeUnsubscribedSubscriber()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            ISubscriberFactory subscriberFactory = A.Fake<ISubscriberFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageBuffer queue = A.Fake<IMessageBuffer>();
            ISubscriber subscriber = A.Fake<ISubscriber>();
            ISubscriptionTag subscriptionTag = A.Fake<ISubscriptionTag>();

            var sut = new Outbox(messageBufferFactory, subscriberFactory, logServiceProvider);

            sut.Unsubscribe(subscriptionTag);

            A.CallTo(() => subscriptionTag.Dispose()).MustNotHaveHappened();
        }

        [Test(Description = "Can unsubscribe the subscriber")]
        public void CanUnsubscribe()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            ISubscriberFactory subscriberFactory = A.Fake<ISubscriberFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageBuffer queue = A.Fake<IMessageBuffer>();
            ISubscriber subscriber = A.Fake<ISubscriber>();
            ISubscriptionTag subscriptionTag = A.Fake<ISubscriptionTag>();

            A.CallTo(() => subscriber.SubscribeTo(A<ISourceBlock<IMessage>>.Ignored)).Returns(subscriptionTag);

            var sut = new Outbox(messageBufferFactory, subscriberFactory, logServiceProvider);
            ISubscriptionTag tag = sut.Subscribe<string>(queue, subscriber);

            sut.Unsubscribe(subscriptionTag);

            A.CallTo(() => subscriptionTag.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}
