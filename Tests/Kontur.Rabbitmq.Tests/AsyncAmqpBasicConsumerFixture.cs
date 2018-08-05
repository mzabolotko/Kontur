using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    internal class AsyncAmqpBasicConsumerFixture
    {
        [Test]
        public void CanLinkTo()
        {
            const string consumerTag = "_consumertag_";

            var connectionFactory = A.Fake<IAmqpConnectionFactory>();
            var propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            var messageBuilder = A.Fake<IAmqpMessageBuilder>();
            var targetBlock = A.Fake<ITargetBlock<IMessage>>();
            var connection = A.Fake<IConnection>();
            var channel = A.Fake<IModel>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateModel()).Returns(channel);
            A.CallTo(() => channel.BasicConsume(
                A<string>.Ignored,
                A<bool>.Ignored,
                A<string>.Ignored,
                A<bool>.Ignored,
                A<bool>.Ignored,
                A<IDictionary<string, object>>.Ignored,
                A<IBasicConsumer>.Ignored)).Returns(consumerTag);

            var sut = new AsyncAmqpBasicConsumer<object>(
                connectionFactory,
                propertyBuilder,
                messageBuilder,
                false,
                "test");

            IPublishingTag tag = sut.LinkTo(targetBlock);
            tag.Id.Should().Be(consumerTag, because: "Id is equal consumer tag.");
        }

        [Test]
        public void CanNotLinkTwice()
        {
            const string consumerTag = "_consumertag_";

            var connectionFactory = A.Fake<IAmqpConnectionFactory>();
            var propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            var messageBuilder = A.Fake<IAmqpMessageBuilder>();
            var targetBlock = A.Fake<ITargetBlock<IMessage>>();
            var connection = A.Fake<IConnection>();
            var channel = A.Fake<IModel>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateModel()).Returns(channel);
            A.CallTo(() => channel.BasicConsume(
                A<string>.Ignored,
                A<bool>.Ignored,
                A<string>.Ignored,
                A<bool>.Ignored,
                A<bool>.Ignored,
                A<IDictionary<string, object>>.Ignored,
                A<IBasicConsumer>.Ignored)).Returns(consumerTag);

            var sut = new AsyncAmqpBasicConsumer<object>(
                connectionFactory,
                propertyBuilder,
                messageBuilder,
                false,
                "test");

            sut.LinkTo(targetBlock);
            ((Action)(() => sut.LinkTo(targetBlock)))
                .Should()
                .Throw<InvalidOperationException>(because: "it is not possible link publisher twice");
        }
    }
}
