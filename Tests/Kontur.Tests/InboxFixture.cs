using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Tests
{
    [TestFixture]
    public class InboxFixture
    {
        [Test(Description = "Can emit message to empty inbox")]
        public void CanEmitToEmptyInbox()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessage message = A.Fake<IMessage>();

            var sut = new Inbox(messageBufferFactory, messageActionFactory, logServiceProvider);

            bool result = sut.EmitAsync<string>(message).Result;

            result.Should().BeFalse(because: "There are no inbox queues of {0} type", typeof(string));
        }

        [Test(Description = "Can emit message to empty inbox")]
        public void CanEmitToInboxWithFullQueue()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageBuffer inboxQueue = A.Fake<IMessageBuffer>();
            IMessage message = A.Fake<IMessage>();

            A.CallTo(() => messageBufferFactory.Create(A<int>.Ignored)).Returns(inboxQueue);
            A.CallTo(() => inboxQueue.SendAsync(A<IMessage>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(false));

            var sut = new Inbox(messageBufferFactory, messageActionFactory, logServiceProvider);
            sut.CreateInboxWithDispatcher<string>(m => { return Task.CompletedTask; });
            bool result = sut.EmitAsync<string>(message).Result;

            result.Should().BeFalse(because: "There is full inbox queue of {0} type", typeof(string));
        }

        [Test(Description = "Can emit message to inbox and get result of message processing")]
        public void CanEmitToInboxAndGetProcessingResult()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageBuffer inboxQueue = A.Fake<IMessageBuffer>();
            IMessage message = A.Fake<IMessage>();
            TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();
            tsc.SetResult(true);

            A.CallTo(() => messageBufferFactory.Create(A<int?>.Ignored)).Returns(inboxQueue);
            A.CallTo(() => inboxQueue.SendAsync(A<IMessage>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));
            A.CallTo(() => message.TaskCompletionSource).Returns(tsc);

            var sut = new Inbox(messageBufferFactory, messageActionFactory, logServiceProvider);
            sut.CreateInboxWithDispatcher<string>(m => { return Task.CompletedTask; });
            bool result = sut.EmitAsync<string>(message).Result;

            result.Should().BeTrue(because: "The message was processed.");
        }

        [Test(Description = "Can get count of the empty inbox")]
        public void CanGetCountEmptyInbox()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();

            var sut = new Inbox(messageBufferFactory, messageActionFactory, logServiceProvider);

            int result = sut.GetInboxMessageCount<string>();

            result.Should().Be(0, because: "There are no inbox queues of {0} type", typeof(string));
        }


        [Test(Description = "Can get count of the inbox")]
        public void CanGetCountInbox()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageBuffer inboxQueue = A.Fake<IMessageBuffer>();

            A.CallTo(() => messageBufferFactory.Create(A<int?>.Ignored)).Returns(inboxQueue);
            const int Value = 100;
            A.CallTo(() => inboxQueue.Count).Returns(Value);

            var sut = new Inbox(messageBufferFactory, messageActionFactory, logServiceProvider);
            sut.CreateInboxWithDispatcher<string>(m => { return Task.CompletedTask; });

            int result = sut.GetInboxMessageCount<string>();

            result.Should().Be(Value, because: "There is the inbox queue of {0} type with 100 messages", typeof(string));
        }

        [Test(Description = "Can create an inbox with the dispatcher")]
        public void CanCreateInboxWithDispatcher()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IMessageAction dispatcher = A.Fake<IMessageAction>();
            IMessageBuffer inboxQueue = A.Fake<IMessageBuffer>();

            A.CallTo(() => messageActionFactory.Create(A<Func<IMessage, Task>>.Ignored)).Returns(dispatcher);
            A.CallTo(() => messageBufferFactory.Create(A<int?>.Ignored)).Returns(inboxQueue);

            var sut = new Inbox(messageBufferFactory, messageActionFactory, logServiceProvider);
            IMessageBuffer result = sut.CreateInboxWithDispatcher<string>(m => { return Task.CompletedTask; });

            A.CallTo(() => inboxQueue.LinkTo(A<IMessageAction>.That.Matches(d => d == dispatcher))).MustHaveHappenedOnceExactly();
            result.Should().Be(inboxQueue, because: "Should be created the inbox.", typeof(string));
        }

        [Test(Description = "Can register publisher in the inbox")]
        public void CanRegisterPublisher()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IPublisher publisher = A.Fake<IPublisher>();
            IMessageBuffer inboxQueue = A.Fake<IMessageBuffer>();

            var sut = new Inbox(messageBufferFactory, messageActionFactory, logServiceProvider);

            IPublishingTag result = sut.RegisterPublisher<string>(publisher, inboxQueue);

            A.CallTo(() => inboxQueue.AsTarget).MustHaveHappenedOnceExactly();
            A.CallTo(() => publisher.LinkTo(A<ITargetBlock<IMessage>>.Ignored)).MustHaveHappenedOnceExactly();

            result.Should().NotBeNull(because: "The publisher should be registered.");
        }

        [Test(Description = "Can check either publisher is registered or not")]
        public void CanCheckIsRegistered()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IPublisher publisher = A.Fake<IPublisher>();
            IMessageBuffer inboxQueue = A.Fake<IMessageBuffer>();

            var sut = new Inbox(messageBufferFactory, messageActionFactory, logServiceProvider);

            IPublishingTag tag = sut.RegisterPublisher<string>(publisher, inboxQueue);
            bool result = sut.IsRegistered(tag);

            result.Should().BeTrue(because: "The publisher was registered.");
        }

        [Test(Description = "Can unregister the registered publisher")]
        public void CanUnregisterRegisteredPublisher()
        {
            IMessageBufferFactory messageBufferFactory = A.Fake<IMessageBufferFactory>();
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = new NUnitLogProvider();
            IPublisher publisher = A.Fake<IPublisher>();
            IMessageBuffer inboxQueue = A.Fake<IMessageBuffer>();
            IPublishingTag publishingTag = A.Fake<IPublishingTag>();

            var sut = new Inbox(messageBufferFactory, messageActionFactory, logServiceProvider);

            sut.Unregister(publishingTag);

            A.CallTo(() => publishingTag.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}
