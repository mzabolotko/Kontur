using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using RabbitMQ.Client;
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

            var sut = new AmqpSender(connectionFactory, messageBuilder);

            ISubscriptionTag tag = sut.SubscribeTo(sourceBlock);
            tag.Id.Should().NotBeNull();
        }
    }
}
