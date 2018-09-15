using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur.Tests
{
    [TestFixture]
    internal class MessageDispatcherFixture
    {
        [Test]
        public void GivenDispatcherWhenSubscribingThenCreateSubscriber()
        {
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();

            var sut = new MessageDispatcher();

            sut.Subscribe<int>(target);

            sut.GetCountSubscriberOf(typeof(int)).Should().Be(1);
        }

        [Test]
        public void GivenDispatcherWhenMulitpleSubscribingWithSameTypeThenCreateMulitpleSubscribers()
        {
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();

            var sut = new MessageDispatcher();

            sut.Subscribe<int>(target);
            sut.Subscribe<int>(target);
            sut.Subscribe<int>(target);

            sut.GetCountSubscriberOf(typeof(int)).Should().Be(3);
        }

        [Test]
        public void GivenSubscriberThenDisposingRemoveSubscriber()
        {
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();

            var sut = new MessageDispatcher();

            IDisposable tag = sut.Subscribe<int>(target);
            tag.Dispose();

            sut.GetCountSubscriberOf(typeof(int)).Should().Be(0);
        }

        [Test]
        public void GivenEmptyListOfSubscribersThenMessageDispathToNobody()
        {
            BufferBlock<IMessage> target = new BufferBlock<IMessage>();
            IMessage message = A.Fake<IMessage>();
            A.CallTo(() => message.RouteKey).Returns(typeof(string));

            var sut = new MessageDispatcher();
            IDisposable tag = sut.Subscribe<int>(target);

            Task task = sut.Dispatch(message);
            task.IsCompleted.Should().BeTrue();
            target.Count.Should().Be(0);
        }

        [Test]
        public void GivenSeveralSameTypeSubscribersThenMEssageDispatchToAll()
        {
            BufferBlock<IMessage> target = new BufferBlock<IMessage>();
            IMessage message = A.Fake<IMessage>();
            A.CallTo(() => message.RouteKey).Returns(typeof(int));

            var sut = new MessageDispatcher();
            sut.Subscribe<int>(target);
            sut.Subscribe<int>(target);

            Task task = sut.Dispatch(message);
            task.IsCompleted.Should().BeTrue();
            target.Count.Should().Be(2);
        }

        [Test]
        public void GivenSeveralDifferentTypeSubscribersThenMessageDistachToOnlyOne()
        {
            BufferBlock<IMessage> target = new BufferBlock<IMessage>();
            IMessage message = A.Fake<IMessage>();
            A.CallTo(() => message.RouteKey).Returns(typeof(int));

            var sut = new MessageDispatcher();
            sut.Subscribe<int>(target);
            sut.Subscribe<char>(target);
            sut.Subscribe<char>(target);
            sut.Subscribe<string>(target);
            sut.Subscribe<double>(target);

            Task task = sut.Dispatch(message);
            task.IsCompleted.Should().BeTrue();
            target.Count.Should().Be(1);
        }
    }
}
