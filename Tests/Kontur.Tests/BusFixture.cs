using FluentAssertions;
using System.Threading;
using NUnit.Framework;
using FakeItEasy;
using System.Threading.Tasks.Dataflow;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Kontur.Tests
{
    [TestFixture]
    internal class BusFixture
    {
        [Test(Description = "Can emit a message to the inbox")]
        public void CanEmitToInbox()
        {
            IInbox inbox = A.Fake<IInbox>();
            IOutbox outbox = A.Fake<IOutbox>();
            IExchange exchange = A.Fake<IExchange>();

            var sut = new Bus(inbox, outbox, exchange, new NUnitLogProvider());

            DoSomethingCommand payload = new DoSomethingCommand();
            IDictionary<string, string> headers = new Dictionary<string, string>
            {
                { "key", "value" }
            };
            sut.EmitAsync(payload, headers).Wait();

            A.CallTo(
                () => inbox.EmitAsync<DoSomethingCommand>(
                    A<IMessage>.That.Matches(m => (m.Payload == payload) && (m.Headers == headers) && (m.TaskCompletionSource != null))))
                        .MustHaveHappenedOnceExactly();
        }

        [Test(Description = "Can subscribe the bus")]
        public void CanSubscribe()
        {
            IInbox inbox = A.Fake<IInbox>();
            IOutbox outbox = A.Fake<IOutbox>();
            IExchange exchange = A.Fake<IExchange>();
            IMessageBuffer queue = A.Fake<IMessageBuffer>();
            ISubscriptionTag tag = A.Fake<ISubscriptionTag>();

            A.CallTo(() => outbox.CreateSubscriberQueue<DoSomethingCommand>(A<int>.Ignored))
                .Returns(queue);
            A.CallTo(() => outbox.Subscribe<DoSomethingCommand>(A<IMessageBuffer>.Ignored, A<Action<Message<DoSomethingCommand>>>.Ignored))
                .Returns(tag);

            var sut = new Bus(inbox, outbox, exchange, new NUnitLogProvider());

            ISubscriptionTag result = sut.Subscribe<DoSomethingCommand>(message => { });

            A.CallTo(() => exchange.BindSubscriberQueue<DoSomethingCommand>(
                A<IInbox>.That.Matches(i => i == inbox),
                A<ITargetBlock<IMessage>>.Ignored))
                    .MustHaveHappenedOnceExactly();

            result.Should().BeSameAs(tag, because: "the bus should be return correct subscription");
        }

        [Test(Description = "Can subscribe the subscriber to the bus.")]
        public void CanSubscribeSubscriber()
        {
            IInbox inbox = A.Fake<IInbox>();
            IOutbox outbox = A.Fake<IOutbox>();
            IExchange exchange = A.Fake<IExchange>();
            ISubscriber subscriber = A.Fake<ISubscriber>();
            IMessageBuffer queue = A.Fake<IMessageBuffer>();
            ISubscriptionTag tag = A.Fake<ISubscriptionTag>();

            A.CallTo(() => outbox.CreateSubscriberQueue<DoSomethingCommand>(A<int>.Ignored))
                .Returns(queue);
            A.CallTo(() => outbox.Subscribe<DoSomethingCommand>(A<IMessageBuffer>.Ignored, A<ISubscriber>.Ignored))
                .Returns(tag);

            var sut = new Bus(inbox, outbox, exchange, new NUnitLogProvider());
            ISubscriptionTag result = sut.Subscribe<DoSomethingCommand>(subscriber);

            A.CallTo(() => exchange.BindSubscriberQueue<DoSomethingCommand>(
                A<IInbox>.That.Matches(i => i == inbox),
                A<ITargetBlock<IMessage>>.Ignored))
                    .MustHaveHappenedOnceExactly();

            result.Should().BeSameAs(tag, because: "the bus should be return correct subscription");
        }

        [Test(Description = "Can get count of inbox messages")]
        public void CanGetInboxMessageCount()
        {
            IInbox inbox = A.Fake<IInbox>();
            IOutbox outbox = A.Fake<IOutbox>();
            IExchange exchange = A.Fake<IExchange>();

            const int Value = 100;
            A.CallTo(() => inbox.GetInboxMessageCount<string>()).Returns(Value);

            var sut = new Bus(inbox, outbox, exchange, new NUnitLogProvider());
            int result = sut.GetInboxMessageCount<string>();

            result.Should().Be(Value, because: "The bus should return the value from the inbox.");
        }

        [Test(Description = "Can check a subscribtion")]
        public void CanCheckIsSubscribed()
        {
            IInbox inbox = A.Fake<IInbox>();
            IOutbox outbox = A.Fake<IOutbox>();
            IExchange exchange = A.Fake<IExchange>();
            ISubscriptionTag tag = A.Fake<ISubscriptionTag>();

            const bool Value = true;
            A.CallTo(() => outbox.IsSubscribed(A<ISubscriptionTag>.That.Matches(t => t == tag))).Returns(Value);

            var sut = new Bus(inbox, outbox, exchange, new NUnitLogProvider());
            bool result = sut.IsSubscribed(tag);

            result.Should().Be(Value, because: "The bus should return the value from the outbox.");
        }

        [Test(Description = "Can unsubscribe")]
        public void CanUnsubscribe()
        {
            IInbox inbox = A.Fake<IInbox>();
            IOutbox outbox = A.Fake<IOutbox>();
            IExchange exchange = A.Fake<IExchange>();
            ISubscriptionTag tag = A.Fake<ISubscriptionTag>();


            var sut = new Bus(inbox, outbox, exchange, new NUnitLogProvider());
            sut.Unsubscribe(tag);
            A.CallTo(() => outbox.Unsubscribe(tag)).MustHaveHappenedOnceExactly();
        }

        [Test(Description = "Can register the publisher into the bus.")]
        public void CanRegisterPublisher()
        {
            IInbox inbox = A.Fake<IInbox>();
            IOutbox outbox = A.Fake<IOutbox>();
            IExchange exchange = A.Fake<IExchange>();
            IPublisher publisher = A.Fake<IPublisher>();
            IMessageBuffer queue = A.Fake<IMessageBuffer>();
            IPublishingTag tag = A.Fake<IPublishingTag>();

            A.CallTo(() => inbox.RegisterPublisher<DoSomethingCommand>(
                A<IPublisher>.That.Matches(p => p == publisher),
                A<IMessageBuffer>.Ignored))
                    .Returns(tag);

            var sut = new Bus(inbox, outbox, exchange, new NUnitLogProvider());
            IPublishingTag result = sut.RegisterPublisher<DoSomethingCommand>(publisher);

            A.CallTo(() => exchange.BindPublisher<DoSomethingCommand>(
                         A<IInbox>.That.Matches(i => i == inbox)))
                            .MustHaveHappenedOnceExactly();

            result.Should().BeSameAs(tag, because: "the bus should be return correct publishingtag");
        }

        [Test(Description = "Can check a registration")]
        public void CanCheckIsRegistered()
        {
            IInbox inbox = A.Fake<IInbox>();
            IOutbox outbox = A.Fake<IOutbox>();
            IExchange exchange = A.Fake<IExchange>();
            IPublishingTag tag = A.Fake<IPublishingTag>();

            const bool Value = true;
            A.CallTo(() => inbox.IsRegistered(A<IPublishingTag>.That.Matches(t => t == tag))).Returns(Value);

            var sut = new Bus(inbox, outbox, exchange, new NUnitLogProvider());
            bool result = sut.IsRegistered(tag);

            result.Should().Be(Value, because: "The bus should return the value from the inbox.");
        }

        [Test(Description = "Can unregister a publisher")]
        public void CanUnregisterPublisher()
        {
            IInbox inbox = A.Fake<IInbox>();
            IOutbox outbox = A.Fake<IOutbox>();
            IExchange exchange = A.Fake<IExchange>();
            IPublishingTag tag = A.Fake<IPublishingTag>();

            var sut = new Bus(inbox, outbox, exchange, new NUnitLogProvider());
            sut.Unregister(tag);
            A.CallTo(() => inbox.Unregister(tag)).MustHaveHappenedOnceExactly();
        }

        [Test(Description = "Can emit a message to the empty bus")]
        [Ignore("Move to integration tests")]
        public void CanEmitToEmptyBus()
        {
            var sut = new Bus(null, null, null, null);

            Assert.DoesNotThrowAsync((async () => await sut.EmitAsync<object>(new object(), null)), "the empty bus will purge emitted messages without subscribers");

            sut.GetInboxMessageCount<object>().Should().Be(0, because: "the empty bus will purge emitted messages without subscribers");
        }

        [Test(Description = "Can emit a message to the single subscriber")]
        [Ignore("Move to integration tests")]
        public void CanEmitToSingleSubscriber()
        {
            ManualResetEventSlim manualEvent = new ManualResetEventSlim(false);
            var sut = new Bus(null, null, null, null);

            sut.Subscribe<DoSomethingCommand>(message => { manualEvent.Set(); });
            sut.EmitAsync(new DoSomethingCommand(), null);

            manualEvent.Wait(10).Should().BeTrue(because: "if the bus emits a message then the subscriber should be called");
        }

        [Test(Description = "Can emit a message to multiple subscribers")]
        [Ignore("Move to integration tests")]
        public void CanEmitToMultipleSubscribers()
        {
            var count = new CountdownEvent(2);
            var sut = new Bus(null, null, null, null);

            sut.Subscribe<DoSomethingCommand>(message => { count.Signal(); });
            sut.Subscribe<DoSomethingCommand>(message => { count.Signal(); });
            sut.EmitAsync(new DoSomethingCommand(), null);

            count.Wait(10).Should().BeTrue("if the bus emits a message then all subscribers should be called");
        }

        [Test(Description = "Can get count of inbox messages for the bus with no subscribers.")]
        [Ignore("Move to integration tests")]
        public void CanGetInboxMessageCountOfNotsubscribedType()
        {
            const int Capacity = 10;
            var sut = new Bus(null, null, null, null);

            for (var i = 0; i < Capacity * Capacity; i++)
            {
                sut.EmitAsync("hello", new Dictionary<string, string>()).Wait();
            }

            sut.GetInboxMessageCount<string>().Should().Be(0);
        }

        [Test(Description = "Can not fail if the subscriber throws an exception.")]
        [Ignore("Move to integration tests")]
        public void CanNotFailDuringSubscriberException()
        {
            var manualEvent = new ManualResetEventSlim(false);
            var messageBufferFactory = new MessageBufferFactory(10);
            var messageActionFactory = new MessageActionFactory();
            var logServiceProvider = new NUnitLogProvider();
            var logService = logServiceProvider.GetLogServiceOf(this.GetType());
            var sut = new Bus(
                        new Inbox(messageBufferFactory, messageActionFactory, logServiceProvider),
                        new Outbox(messageBufferFactory, messageActionFactory, logServiceProvider),
                        new Exchange(logServiceProvider),
                        logServiceProvider);

            var i = 0;
            sut.Subscribe<string>(m =>
            {
                logService.Trace("Message - {0}, index - {1}.", m.Payload, i);
                if (i == 0)
                {

                    i++;
                    logService.Trace("Throw exception.");
                    throw new Exception();
                }
                else
                {
                    manualEvent.Set();
                }
            });

            Task<bool> firstTask = sut.EmitAsync("first", new Dictionary<string, string>()); //.Wait();
            Task<bool> secondTask = sut.EmitAsync("second", new Dictionary<string, string>()); //.Wait();

            manualEvent.Wait();
            manualEvent.IsSet.Should().BeTrue();
            firstTask.Result.Should().BeTrue(because: "The subscriber exists");
            secondTask.Result.Should().BeTrue(because: "The subscriber exists");
        }
    }

    internal class DoSomethingCommand
    {
    }
}
