using FluentAssertions;
using System.Threading;
using NUnit.Framework;
using FakeItEasy;
using System.Threading.Tasks.Dataflow;

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

            sut.InboxMessageCount.Should().Be(0, because: "the empty bus will purge emitted messages without subscribers");
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
            A.CallTo(() => subscriber.LinkTo(A<ISourceBlock<IMessage>>.Ignored)).MustHaveHappenedOnceExactly();
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

            IPublishingTag tag = sut.RegisterPublisher(publisher);
            tag.Should().NotBeNull();
            A.CallTo(() => publisher.LinkTo(A<ITargetBlock<IMessage>>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }

    internal class DoSomethingCommand
    {

    }
}
