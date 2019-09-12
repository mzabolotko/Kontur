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
        [Test(Description = "Can subscribe to the dispatcher")]
        public void CanSubscribeToDispatcher()
        {
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();

            var sut = new MessageDispatcher(logServiceProvider);

            sut.Subscribe<int>(target);

            sut.GetCountSubscriberOf(typeof(int)).Should().Be(1);
        }

        [Test(Description = "Can subscribe to the dispatcher with the same message type multiple times")]
        public void CanSubscribeToDispatcherWithSameMessageType()
        {
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();

            var sut = new MessageDispatcher(logServiceProvider);

            sut.Subscribe<int>(target);
            sut.Subscribe<int>(target);
            sut.Subscribe<int>(target);

            sut.GetCountSubscriberOf(typeof(int)).Should().Be(3);
        }

        [Test(Description = "Can dispose the subscriber from the dispatcher")]
        public void CanDisposeSubscriber()
        {
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();

            var sut = new MessageDispatcher(logServiceProvider);

            IDisposable tag = sut.Subscribe<int>(target);
            tag.Dispose();

            sut.GetCountSubscriberOf(typeof(int)).Should().Be(0);
        }

        [Test(Description = "Can dispatch message to nobody")]
        public void CanDispatchMessageToNobody()
        {
            IMessage message = A.Fake<IMessage>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            A.CallTo(() => message.RouteKey).Returns(typeof(string));

            var sut = new MessageDispatcher(logServiceProvider);

            Task task = sut.Dispatch(message);
            task.IsCompleted.Should().BeTrue(because: "there are no any subscribers and the dispatching task have to completed now");
        }

        [Test(Description = "Can dispatch a message to all subscribers")]
        public void CanDispatchMessageToAllSubscribers()
        {
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();
            IMessage message = A.Fake<IMessage>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            A.CallTo(() => message.RouteKey).Returns(typeof(int));

            var sut = new MessageDispatcher(logServiceProvider);
            sut.Subscribe<int>(target);
            sut.Subscribe<int>(target);

            Task task = sut.Dispatch(message);
            task.IsCompleted.Should().BeTrue();
            A.CallTo(() => target.OfferMessage(
                                A<DataflowMessageHeader>.Ignored,
                                A<IMessage>.That.Matches(m => message == m),
                                A<ISourceBlock<IMessage>>.Ignored,
                                A<bool>.Ignored))
                .MustHaveHappenedTwiceExactly();
        }

        [Test(Description = "Can dispatch a message to a correct subscriber")]
        public void CanDispatchToCorrectSubuscriber()
        {
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();
            ITargetBlock<IMessage> anotherTarget = A.Fake<ITargetBlock<IMessage>>();
            IMessage message = A.Fake<IMessage>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            A.CallTo(() => message.RouteKey).Returns(typeof(int));

            var sut = new MessageDispatcher(logServiceProvider);
            sut.Subscribe<int>(target);
            sut.Subscribe<char>(anotherTarget);
            sut.Subscribe<char>(anotherTarget);
            sut.Subscribe<string>(anotherTarget);
            sut.Subscribe<double>(anotherTarget);

            Task task = sut.Dispatch(message);
            task.IsCompleted.Should().BeTrue();
            A.CallTo(() => target.OfferMessage(
                                A<DataflowMessageHeader>.Ignored,
                                A<IMessage>.That.Matches(m => message == m),
                                A<ISourceBlock<IMessage>>.Ignored,
                                A<bool>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => anotherTarget.OfferMessage(
                                A<DataflowMessageHeader>.Ignored,
                                A<IMessage>.That.Matches(m => message == m),
                                A<ISourceBlock<IMessage>>.Ignored,
                                A<bool>.Ignored))
                .MustNotHaveHappened();
        }

        [Test(Description = "Can unsubscribe event if all subscribers given type have been unsubcribed already")]
        public void CanUnsubcribeUnsubscribedType()
        {
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();

            var sut = new MessageDispatcher(logServiceProvider);

            IDisposable tag = sut.Subscribe<int>(target);
            tag.Dispose();

            Action act = () => tag.Dispose();

            act.Should().NotThrow(because: "an unsubscribe operation should be idempotent");
        }

        [Test(Description = "Can unsubscribe event if the current subscriber have been unsubcribed already")]
        public void CanUnsubcribeUnsubscribedSubscribers()
        {
            ITargetBlock<IMessage> target = A.Fake<ITargetBlock<IMessage>>();
            ITargetBlock<IMessage> target2 = A.Fake<ITargetBlock<IMessage>>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();

            var sut = new MessageDispatcher(logServiceProvider);

            IDisposable tag = sut.Subscribe<int>(target);
            IDisposable tag2 = sut.Subscribe<int>(target2);
            tag.Dispose();

            Action act = () => tag.Dispose();

            act.Should().NotThrow(because: "an unsubscribe operation should be idempotent");
        }
    }
}
