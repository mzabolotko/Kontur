using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    internal class AmqpSenderFixture
    {
        [Test]
        public void CanLinkTo()
        {
            // Assert
            var connectionFactory = A.Fake<IAmqpConnectionFactory>();
            var messageBuilder = A.Fake<AmqpMessageBuilder>();
            var sourceBlock = A.Fake<ISourceBlock<IMessage>>();
            var connection = A.Fake<IConnection>();
            var channel = A.Fake<IModel>();

            A.CallTo(() => connectionFactory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateModel()).Returns(channel);

            var sut = new AmqpSender(connectionFactory, messageBuilder, new LogServiceProvider());

            // Act
            ISubscriptionTag tag = sut.SubscribeTo(sourceBlock);

            // Assert
            tag.Id.Should().NotBeNull();
        }

        [Test(Description = "Can send message after transform exception.")]
        public async Task CanSendMessageWithSerializationException()
        {
            // Arrange
            var channel = A.Fake<IModel>();
            A.CallTo(() => channel.CreateBasicProperties())
                .Returns(null);

            A.CallTo(() => channel.BasicPublish(
                    A<string>._,
                    A<string>._,
                    A<bool>._,
                    A<IBasicProperties>._,
                    A<byte[]>._));

            IAmqpConnectionFactory connectionFactory = GetConnectionFactory(channel);

            var tasks = new List<TaskCompletionSource<bool>>()
            {
                new TaskCompletionSource<bool>(),
                new TaskCompletionSource<bool>()
            };

            var properties = A.Fake<IAmqpProperties>();
            var messageBuilder = A.Fake<AmqpMessageBuilder>();
            A.CallTo(() => messageBuilder.Serialize(A<IMessage>._))
                .Throws<Exception>()
                .Once()
                .Then
                .Returns(new AmqpMessage(properties, null, null, new byte[1], tasks[1]));

            var sut = new AmqpSender(connectionFactory, messageBuilder, new LogServiceProvider());
            var input = new BufferBlock<IMessage>();
            sut.SubscribeTo(input);

            // Act
            input.Post(new Message<string>("hello", new Dictionary<string, string>(), tasks[0]));
            input.Post(new Message<string>("hello", new Dictionary<string, string>(), tasks[1]));

            // Assert
            (await tasks[0].Task).Should().Be(false);
            (await tasks[1].Task).Should().Be(true);
        }

        [Test(Description = "Can send message after sent exception.")]
        public async Task CanSendMessageAfterSentException()
        {
            // Arrange
            var channel = A.Fake<IModel>();
            A.CallTo(() => channel.CreateBasicProperties())
                .Returns(null);

            A.CallTo(() => channel.BasicPublish(
                    A<string>._,
                    A<string>._,
                    A<bool>._,
                    A<IBasicProperties>._,
                    A<byte[]>._))
                .Throws<Exception>()
                .Once();

            IAmqpConnectionFactory connectionFactory = GetConnectionFactory(channel);

            var tasks = new List<TaskCompletionSource<bool>>()
            {
                new TaskCompletionSource<bool>(),
                new TaskCompletionSource<bool>()
            };

            var properties = A.Fake<IAmqpProperties>();
            var messageBuilder = A.Fake<AmqpMessageBuilder>();
            A.CallTo(() => messageBuilder.Serialize(A<IMessage>._))
                .ReturnsNextFromSequence(new AmqpMessage[]
                {
                    new AmqpMessage(properties, null, null, new byte[1], tasks[0]),
                    new AmqpMessage(properties, null, null, new byte[1], tasks[1])
                });

            var sut = new AmqpSender(connectionFactory, messageBuilder, new LogServiceProvider());

            var input = new BufferBlock<IMessage>();
            sut.SubscribeTo(input);

            // Act
            input.Post(new Message<string>("hello", new Dictionary<string, string>(), tasks[0]));
            input.Post(new Message<string>("hello", new Dictionary<string, string>(), tasks[1]));

            // Assert
            await tasks[0].Task.ContinueWith(t =>
            {
                t.Status.Should().Be(TaskStatus.Faulted);
            });

            (await tasks[1].Task).Should().Be(true);
        }

        private static IAmqpConnectionFactory GetConnectionFactory(IModel channel)
        {
            var connection = A.Fake<IConnection>();
            A.CallTo(() => connection.CreateModel())
                .Returns(channel);

            var connectionFactory = A.Fake<IAmqpConnectionFactory>();
            A.CallTo(() => connectionFactory.CreateConnection())
                .Returns(connection);

            return connectionFactory;
        }
    }
}
