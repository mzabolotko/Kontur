using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    public class AmqpProptertiesFixture
    {
        [Test(Description = "Can copy properties")]
        public void CanCopyBasicProperites()
        {
            const string ContentType = "text/plain";
            const string ContentEncoding = "utf-8";
            const string CorrelationId = "336dadf34";
            const string MessageId = "6de45affdc";
            const bool Persistent = true;
            const string ReplyTo = "test";
            Dictionary<string, string> Headers = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
            AmqpProperties sut = new AmqpProperties
            {
                ContentType = ContentType,
                ContentEncoding = ContentEncoding,
                CorrelationId = CorrelationId,
                MessageId = MessageId,
                Persistent = Persistent,
                ReplyTo = ReplyTo,
                Headers = Headers
            };

            IBasicProperties basicProperties = A.Fake<IBasicProperties>();

            sut.CopyTo(basicProperties);

            basicProperties
                .ContentType
                    .Should()
                        .Be(ContentType, because: "Property should be copied.");
            basicProperties
                .ContentEncoding
                    .Should()
                        .Be(ContentEncoding, because: "Property should be copied.");
            basicProperties
                .CorrelationId
                    .Should()
                        .Be(CorrelationId, because: "Property should be copied.");
            basicProperties
                .MessageId
                    .Should()
                        .Be(MessageId, because: "Property should be copied.");
            basicProperties
                .Persistent
                    .Should()
                        .Be(Persistent, because: "Property should be copied.");
            basicProperties
                .ReplyTo
                    .Should()
                        .Be(ReplyTo, because: "Property should be copied.");
            basicProperties
                .Headers.Should().HaveCount(Headers.Count, because: "Property should be copied.");
        }
    }
}
