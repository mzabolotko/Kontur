using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading;
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
            const string ConsumerTag = "_consumertag_";

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
                A<IBasicConsumer>.Ignored)).Returns(ConsumerTag);

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

        [Test(Description = "Can consume an another message after deserialization exception of the former message")]
        public void CanConsumeAfterDeserializingException()
        {
            const string ConsumerTag = "_consumertag_";

            var connectionFactory = A.Fake<IAmqpConnectionFactory>();
            var propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            var messageBuilder = A.Fake<IAmqpMessageBuilder>();
            var targetBlock = new BufferBlock<IMessage>();
            var connection = A.Fake<IConnection>();
            var channel = A.Fake<IModel>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateModel()).Returns(channel);

            A.CallTo(() => messageBuilder.Deserialize<string>(A<AmqpMessage>.Ignored))
                .Throws<Exception>()
                .Once()
                .Then
                .Returns(new Message<string>("hello", new Dictionary<string, string>()));

            var sut = new AsyncAmqpBasicConsumer<string>(
                connectionFactory,
                propertyBuilder,
                messageBuilder,
                false,
                "test");

            using (var link = sut.LinkTo(targetBlock))
            {
                sut.OnReceived(channel, new BasicDeliverEventArgs(ConsumerTag, 100, false, "test", string.Empty, null, null)).Wait();
                sut.OnReceived(channel, new BasicDeliverEventArgs(ConsumerTag, 100, false, "test", string.Empty, null, null)).Wait();

                ((Action) (() => targetBlock.Receive(TimeSpan.FromMilliseconds(10)))).Should().NotThrow<Exception>();
            }
        }

        [Test(Description = "Can noack an delivery after deserialization exception.")]
        public void CanNoackAfterDeserializingException()
        {
            const string ConsumerTag = "_consumertag_";

            var manualReset = new ManualResetEventSlim(false);

            var connectionFactory = A.Fake<IAmqpConnectionFactory>();
            var propertyBuilder = A.Fake<IAmqpPropertyBuilder>();
            var messageBuilder = A.Fake<IAmqpMessageBuilder>();
            var targetBlock = new BufferBlock<IMessage>();
            var connection = A.Fake<IConnection>();
            var channel = A.Fake<IModel>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateModel()).Returns(channel);

            A.CallTo(() => messageBuilder.Deserialize<string>(A<AmqpMessage>.Ignored))
                .Throws<Exception>();

            var sut = new AsyncAmqpBasicConsumer<string>(
                connectionFactory,
                propertyBuilder,
                messageBuilder,
                false,
                "test");

            const ulong DeliveryTag = 100;

            using (var link = sut.LinkTo(targetBlock))
            {
                sut.OnReceived(
                    channel,
                    new BasicDeliverEventArgs(
                        ConsumerTag,
                        DeliveryTag,
                        false,
                        "test",
                        string.Empty,
                        null,
                        null)).Wait();

                A.CallTo(() => channel.BasicNack(A<ulong>.That.IsEqualTo(DeliveryTag), false, false))
                    .MustHaveHappened();
            }
        }

    }
}
