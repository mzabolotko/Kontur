﻿using FluentAssertions;
using System;
using System.Threading;
using Xunit;

namespace Kontur.Tests
{
    public class BusFixture
    {
        [Fact(DisplayName = "Can emit a message to the empty bus")]
        public void CanEmitToEmptyBus()
        {
            var sut = new Bus();

            ((Action)(async () => await sut.EmitAsync<object>("test", new object())))
                .Should().NotThrow(because:"the empty bus will purge emitted messages without subscribers");

            sut.InboxMessageCount.Should().Be(0, because:"the empty bus will purge emitted messages without subscribers");
        }

        [Fact(DisplayName = "Can emit a message to the single subscriber")]
        public void CanEmitToSingleSubscriber()
        {
            const string CommandDoSomething = "command.do.something";

            var manualEvent = new ManualResetEventSlim(false);
            var sut = new Bus();

            sut.Subscribe(CommandDoSomething, message => { manualEvent.Set(); });
            sut.EmitAsync(CommandDoSomething, new object()).Wait();

            manualEvent.Wait(10).Should().BeTrue(because:"if the bus emits a message then the subscriber should be called");
        }

        [Fact(DisplayName = "Can emit a message to multiple subscribers")]
        public void CanEmitToMultipleSubscribers()
        {
            const string CommandDoSomething = "command.do.something";

            var count = new CountdownEvent(2);
            var sut = new Bus();

            sut.Subscribe(CommandDoSomething, message => { count.Signal(); });
            sut.Subscribe(CommandDoSomething, message => { count.Signal(); });
            sut.EmitAsync(CommandDoSomething, new object()).Wait();

            count.Wait(10).Should().BeTrue("if the bus emits a message then all subscribers should be called");
        }

        [Fact(DisplayName = "Can subscribe the bus")]
        public void CanSubscribe()
        {
            const string CommandDoSomething = "command.do.something";

            var sut = new Bus();
            ISubscriptionTag tag = sut.Subscribe(CommandDoSomething, message => { });
            sut.IsSubscribed(tag).Should().BeTrue(because:"the bus creates the subscription");
        }

        [Fact(DisplayName = "Can unsubscribe from the bus")]
        public void CanUnsubscribe()
        {
            const string CommandDoSomething = "command.do.something";

            var sut = new Bus();
            ISubscriptionTag tag = sut.Subscribe(CommandDoSomething, message => { });

            sut.Unsubscribe(tag);
            sut.IsSubscribed(tag).Should().BeFalse(because: "the bus unsubscribe it");
        }
    }
}