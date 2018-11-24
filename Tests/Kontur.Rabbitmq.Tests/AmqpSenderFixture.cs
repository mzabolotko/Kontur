using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
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


        [Test(Description = "Can send message after transform exception.")]
        public void CanSendMessageWithSerializationException()
        {
            IAmqpConnectionFactory connectionFactory = A.Fake<IAmqpConnectionFactory>();
            AmqpMessageBuilder messageBuilder = A.Fake<AmqpMessageBuilder>();
            ISourceBlock<IMessage> sourceBlock = A.Fake<ISourceBlock<IMessage>>();
            IConnection connection = A.Fake<IConnection>();
            IAmqpProperties properties = A.Fake<IAmqpProperties>();
            IModel channel = A.Fake<IModel>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateModel()).Returns(channel);

            A.CallTo(() => messageBuilder.Serialize(A<IMessage>._))
                .Throws<Exception>()
                .Once()
                .Then.
                Returns(new AmqpMessage(properties, null, null, null));

            var sut = new AmqpSender(connectionFactory, messageBuilder, new LogServiceProvider());
            IMessage message = new Message<string>("hello", new Dictionary<string, string>());
            Action action = () => sut.Transform(message);
            action.Should().NotThrow("because the AmqpSender should catch all exception to prevent from destroying the DataFlow chain.");
        }

        [Test(Description = "Can send message after sent exception.")]
        public void CanSendMessageWithSentException()
        {
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
                .Once();

            A.CallTo(() => messageBuilder.Serialize(A<IMessage>._))
                .Returns(new AmqpMessage(properties, null, null, null)).Twice();

            var sut = new AmqpSender(connectionFactory, messageBuilder, new LogServiceProvider());

            Result<AmqpMessage, ExceptionDispatchInfo> result =
                new Result<AmqpMessage, ExceptionDispatchInfo>(
                         new AmqpMessage(new AmqpProperties(), string.Empty, string.Empty, new byte[0]));

            Action action = () => sut.Send(result);
            action.Should().NotThrow("because the AmqpSender should catch all exceptions to prevent from destroying of the DataFlow chain.");
        }
    }
}
