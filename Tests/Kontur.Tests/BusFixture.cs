using FluentAssertions;
using System.Threading;
using NUnit.Framework;
using FakeItEasy;
using System.Threading.Tasks.Dataflow;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Kontur.Tests
{
    [TestFixture]
    internal class BusFixture
    {
        [Test(Description = "Can emit a message to the empty bus")]
        public void CanEmitToEmptyBus()
        {
            var sut = new Bus();

            Assert.DoesNotThrowAsync((async () => await sut.EmitAsync<object>(new object(), null)), "the empty bus will purge emitted messages without subscribers");

            sut.GetInboxMessageCount<object>().Should().Be(0, because: "the empty bus will purge emitted messages without subscribers");
        }

        [Test(Description = "Can emit a message to the single subscriber")]
        public void CanEmitToSingleSubscriber()
        {
            var manualEvent = new ManualResetEventSlim(false);
            var sut = new Bus();

            sut.Subscribe<DoSomethingCommand>(message => { manualEvent.Set(); });
            sut.EmitAsync(new DoSomethingCommand(), null).Wait();

            manualEvent.Wait(10).Should().BeTrue(because: "if the bus emits a message then the subscriber should be called");
        }

        [Test(Description = "Can emit a message to multiple subscribers")]
        public void CanEmitToMultipleSubscribers()
        {
            var count = new CountdownEvent(2);
            var sut = new Bus();

            sut.Subscribe<DoSomethingCommand>(message => { count.Signal(); });
            sut.Subscribe<DoSomethingCommand>(message => { count.Signal(); });
            sut.EmitAsync(new DoSomethingCommand(), null).Wait();

            count.Wait(10).Should().BeTrue("if the bus emits a message then all subscribers should be called");
        }

        [Test(Description = "Can subscribe the bus")]
        public void CanSubscribe()
        {
            var sut = new Bus();
            ISubscriptionTag tag = sut.Subscribe<DoSomethingCommand>(message => { });
            sut.IsSubscribed(tag).Should().BeTrue(because: "the bus creates the subscription");
        }

        [Test(Description = "Can subscribe the subscriber to the bus.")]
        public void CanSubscribeSubscriber()
        {
            ISubscriber subscriber = A.Fake<ISubscriber>();

            var sut = new Bus();
            ISubscriptionTag tag = sut.Subscribe<string>(subscriber);

            tag.Should().NotBeNull();
            A.CallTo(() => subscriber.SubscribeTo(A<ISourceBlock<IMessage>>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Test(Description = "Can unsubscribe from the bus")]
        public void CanUnsubscribe()
        {
            var sut = new Bus();
            ISubscriptionTag tag = sut.Subscribe<DoSomethingCommand>(message => { });

            sut.Unsubscribe(tag);
            sut.IsSubscribed(tag).Should().BeFalse(because: "the bus unsubscribe it");
        }

        [Test(Description = "Can register publisher to the bus.")]
        public void CanRegisterPublisher()
        {
            IPublisher publisher = A.Fake<IPublisher>();
            var sut = new Bus();

            IPublishingTag tag = sut.RegisterPublisher<int>(publisher);
            tag.Should().NotBeNull();
            A.CallTo(() => publisher.LinkTo(A<ITargetBlock<IMessage>>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Test(Description = "Can check is publisher registered.")]
        public void CanCheckIsRegistered()
        {
            IPublishingTag publishingTag = A.Fake<IPublishingTag>();
            IPublisher publisher = A.Fake<IPublisher>();
            A.CallTo(() => publisher.LinkTo(A<ITargetBlock<IMessage>>.Ignored)).Returns(publishingTag);

            var sut = new Bus();
            IPublishingTag tag = sut.RegisterPublisher<int>(publisher);

            sut.IsRegistered(tag).Should().BeTrue();
        }

        [Test(Description = "Can unregister regestered publisher.")]
        public void CanUnregister()
        {
            IPublishingTag publishingTag = A.Fake<IPublishingTag>();
            IPublisher publisher = A.Fake<IPublisher>();
            A.CallTo(() => publisher.LinkTo(A<ITargetBlock<IMessage>>.Ignored)).Returns(publishingTag);

            var sut = new Bus();
            IPublishingTag tag = sut.RegisterPublisher<int>(publisher);

            sut.IsRegistered(tag).Should().BeTrue();
            sut.Unregister(tag);
            sut.IsRegistered(tag).Should().BeFalse();
        }

        [Test(Description = "Can get count of inbox messages for the bus with no subscribers.")]
        public void CanGetInboxMessageCountOfNotsubscribedType()
        {
            const int Capacity = 10;
            var sut = new Bus();

            for (var i = 0; i < Capacity * Capacity; i++)
            {
                sut.EmitAsync("hello", new Dictionary<string, string>()).Wait();
            }

            sut.GetInboxMessageCount<string>().Should().Be(0);
        }

        [Test(Description = "Can block incoming messages if the inbox capacity is exceeded.")]
        public void CanBlockIncomingMessagesWhenCapacityExceeds()
        {
            const int InboxCapacity = 10;
            const int QueueCapacity = 7;
            const int IntermediateBlocks = 2;
            var sut = new Bus(InboxCapacity);
            var manualResetEvent = new ManualResetEvent(false);
            sut.Subscribe<string>(m => manualResetEvent.WaitOne(), QueueCapacity);
            sut.Subscribe<string>(m => manualResetEvent.WaitOne(), QueueCapacity);

            const int taskCount = ((InboxCapacity + QueueCapacity + IntermediateBlocks) * 2);
            var sents =
                Enumerable.Range(1, taskCount)
                .Select(i => sut.EmitAsync("hello", new Dictionary<string, string>()))
                .ToList();

            Thread.Sleep(50);

            var completed = sents.Where(t => t.IsCompleted).Count();
            var success = sents.Where(t => t.IsCompleted).Where(t => t.Result).Count();

            completed.Should().Be(InboxCapacity + QueueCapacity + IntermediateBlocks);
            success.Should().Be(InboxCapacity + QueueCapacity + IntermediateBlocks);

            manualResetEvent.Set();
            Thread.Sleep(50);

            completed = sents.Where(t => t.IsCompleted).Count();
            success = sents.Where(t => t.IsCompleted).Where(t => t.Result).Count();

            completed.Should().Be(taskCount);
            success.Should().Be(taskCount);
        }

        [Test(Description = "Can not fail if the subscriber throws an exception.")]
        public void CanNotFailDuringSubscriberException()
        {
            var manualEvent = new ManualResetEventSlim(false);
            var sut = new Bus(10, new ConsoleLogService());

            var i = 0;
            sut.Subscribe<string>(m =>
            {
                if (i == 0)
                {
                    i++;
                    throw new Exception();
                }
                else
                {
                    manualEvent.Set();
                }
            });


            sut.EmitAsync("hello", new Dictionary<string, string>()).Wait();
            sut.EmitAsync("hello", new Dictionary<string, string>()).Wait();

            manualEvent.Wait(10).Should().BeTrue();
            manualEvent.IsSet.Should().BeTrue();
        }
    }

    internal class DoSomethingCommand
    {

    }
}
