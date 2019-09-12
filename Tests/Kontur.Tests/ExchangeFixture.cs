using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Tests
{
    [TestFixture]
    public class ExchangeFixture
    {
        [Test(Description = "Can bind subscriber queue to the inbox")]
        public void CanBindSubscriberQueue()
        {
            IMessageDispatcherFactory messageDispatcherFactory = A.Fake<IMessageDispatcherFactory>();
            ILogServiceProvider logServiceProvder = A.Fake<ILogServiceProvider>();
            IInbox inbox = A.Fake<IInbox>();
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();
            IMessageDispatcher messageDispatcher = A.Fake<IMessageDispatcher>();

            A.CallTo(() => messageDispatcherFactory.Create()).Returns(messageDispatcher);

            var sut = new Exchange(messageDispatcherFactory, logServiceProvder);

            ISubscriptionTag result = sut.BindSubscriberQueue<string>(inbox, target);

            A.CallTo(() => inbox.CreateInboxWithDispatcher<string>(A<Func<IMessage, Task>>.Ignored))
                .MustHaveHappenedOnceExactly();

            result.Should().NotBeNull();
        }

        [Test(Description = "Can bind publisher to the inbox")]
        public void CanBindPublisher()
        {
            IMessageDispatcherFactory messageDispatcherFactory = A.Fake<IMessageDispatcherFactory>();
            ILogServiceProvider logServiceProvder = A.Fake<ILogServiceProvider>();
            IInbox inbox = A.Fake<IInbox>();
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();
            IMessageDispatcher messageDispatcher = A.Fake<IMessageDispatcher>();

            A.CallTo(() => messageDispatcherFactory.Create()).Returns(messageDispatcher);

            var sut = new Exchange(messageDispatcherFactory, logServiceProvder);

            IMessageBuffer result = sut.BindPublisher<string>(inbox);

            A.CallTo(() => inbox.CreateInboxWithDispatcher<string>(A<Func<IMessage, Task>>.Ignored))
                .MustHaveHappenedOnceExactly();

            result.Should().NotBeNull();
        }
    }
}
