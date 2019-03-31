using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    internal class AmqpSenderFixture
    {
        [Test]
        public void CanLinkTo()
        {
            IAmqpConnectionFactory connectionFactory = A.Fake<IAmqpConnectionFactory>();
            AmqpMessageBuilder messageBuilder = A.Fake<AmqpMessageBuilder>();
            ISourceBlock<IMessage> sourceBlock = A.Fake<ISourceBlock<IMessage>>();
            IConnection connection = A.Fake<IConnection>();
            IModel channel = A.Fake<IModel>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateModel()).Returns(channel);

            var sut = new AmqpSender(connectionFactory, messageBuilder, new LogServiceProvider());

            ISubscriptionTag tag = sut.SubscribeTo(sourceBlock);
            tag.Id.Should().NotBeNull();
        }

        [Test(Description = "Can send message after serialization exception.")]
        public void CanSendMessageWithSerializationException()
        {
            var manualReset = new ManualResetEventSlim(false);

            IAmqpConnectionFactory connectionFactory = A.Fake<IAmqpConnectionFactory>();
            AmqpMessageBuilder messageBuilder = A.Fake<AmqpMessageBuilder>();
            ISourceBlock<IMessage> sourceBlock = A.Fake<ISourceBlock<IMessage>>();
            IConnection connection = A.Fake<IConnection>();
            IAmqpProperties properties = A.Fake<IAmqpProperties>();
            IModel channel = A.Fake<IModel>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateModel()).Returns(channel);
            A.CallTo(() => channel.CreateBasicProperties()).Returns(null);
            A.CallTo(() => channel.BasicPublish(
                                A<string>._,
                                A<string>._,
                                A<bool>._,
                                A<IBasicProperties>._,
                                A<byte[]>._))
                .Invokes(() => manualReset.Set());

            A.CallTo(() => messageBuilder.Serialize(A<IMessage>._))
                .Throws<Exception>()
                .Once()
                .Then
                .Returns(new AmqpMessage(properties, null, null, null));

            var sut = new AmqpSender(connectionFactory, messageBuilder, new LogServiceProvider());

            var input = new BufferBlock<IMessage>();
            sut.SubscribeTo(input);
            input.Post(new Message<string>("hello", new Dictionary<string, string>()));
            input.Post(new Message<string>("hello", new Dictionary<string, string>()));

            manualReset.Wait(10).Should().BeTrue();
            manualReset.IsSet.Should().BeTrue();
        }

        [Test(Description = "Can send message after sent exception.")]
        public void CanSendMessageWithSentException()
        {
            var manualReset = new ManualResetEventSlim(false);

            IAmqpConnectionFactory connectionFactory = A.Fake<IAmqpConnectionFactory>();
            AmqpMessageBuilder messageBuilder = A.Fake<AmqpMessageBuilder>();
            ISourceBlock<IMessage> sourceBlock = A.Fake<ISourceBlock<IMessage>>();
            IConnection connection = A.Fake<IConnection>();
            IAmqpProperties properties = A.Fake<IAmqpProperties>();
            IModel channel = A.Fake<IModel>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateModel()).Returns(channel);
            A.CallTo(() => channel.CreateBasicProperties()).Returns(null);
            A.CallTo(() => channel.BasicPublish(
                                A<string>._,
                                A<string>._,
                                A<bool>._,
                                A<IBasicProperties>._,
                                A<byte[]>._))
                .Throws<Exception>()
                .Once()
                .Then
                .Invokes(() => manualReset.Set());

            A.CallTo(() => messageBuilder.Serialize(A<IMessage>._))
                .Returns(new AmqpMessage(properties, null, null, null)).Twice();

            var sut = new AmqpSender(connectionFactory, messageBuilder, new LogServiceProvider());

            var input = new BufferBlock<IMessage>();
            sut.SubscribeTo(input);
            input.Post(new Message<string>("hello", new Dictionary<string, string>()));
            input.Post(new Message<string>("hello", new Dictionary<string, string>()));

            manualReset.Wait(10).Should().BeTrue();
            manualReset.IsSet.Should().BeTrue();
        }
    }
}
