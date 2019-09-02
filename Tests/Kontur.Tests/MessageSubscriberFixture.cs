using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Tests
{
    [TestFixture]
    internal class MessageSubscriberFixture
    {
        [Test(Description = "Can process message after subscribing.")]
        public void CanProcessSubscribedMessage()
        {
            var manualReset = new ManualResetEvent(false);
            var messageActionFactory = new MessageActionFactory();
            var sut = new MessageSubscriber<string>(m => { manualReset.Set(); }, messageActionFactory, new NUnitLogProvider());

            var input = new BufferBlock<IMessage>();
            sut.SubscribeTo(input);
            input.Post(new Message<string>("hello", new Dictionary<string, string>()));

            manualReset.WaitOne(10).Should().BeTrue();
        }

        [Test(Description = "Can process message after exception.")]
        public void CanProcessSubscribedMessageWithException()
        {
            var thrown = false;
            var manualReset = new ManualResetEventSlim(false);
            var messageActionFactory = new MessageActionFactory();
            var logServiceProvider = new NUnitLogProvider();
            ILogService logService = logServiceProvider.GetLogServiceOf(this.GetType());
            var sut = new MessageSubscriber<string>(m =>
            {
                logService.Trace("Message - {0}, exception thrown - {1}.", m.Payload, thrown);
                if (!thrown)
                {
                    thrown = true;
                    logService.Trace("Throw exception.");
                    throw new Exception();
                }
                else
                {
                    manualReset.Set();
                }
            }, messageActionFactory, logServiceProvider);

            var input = new BufferBlock<IMessage>();
            sut.SubscribeTo(input);

            input.Post(new Message<string>("first", new Dictionary<string, string>()));
            input.Post(new Message<string>("second", new Dictionary<string, string>()));

            manualReset.Wait(100).Should().BeTrue();
            manualReset.IsSet.Should().BeTrue();
        }
    }
}
