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
            var sut = new MessageSubscriber<string>(m => manualReset.Set());

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
            var sut = new MessageSubscriber<string>(m => 
            {
                if (!thrown) 
                {
                    thrown = true;
                    throw new Exception();
                }
                else
                {
                    manualReset.Set();
                }
            });

            var input = new BufferBlock<IMessage>();
            sut.SubscribeTo(input);
            input.Post(new Message<string>("hello", new Dictionary<string, string>()));
            input.Post(new Message<string>("hello", new Dictionary<string, string>()));

            manualReset.Wait(10).Should().BeTrue();
            manualReset.IsSet.Should().BeTrue();
        }        
    }
}