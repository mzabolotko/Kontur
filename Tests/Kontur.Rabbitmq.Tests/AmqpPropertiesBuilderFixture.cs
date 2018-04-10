using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    class AmqpPropertiesBuilderFixture
    {
        [Test(Description = "Can create empty amqp properties")]
        public void CanCreateEmptyAmqpProperties()
        {
            IMessage message = A.Fake<IMessage>();
            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.Build(message);

            properties.Should().NotBeNull(because: "Builder can build empty properties");
        }

        [Test(Description = "Can create amqp properties with content endcoding.")]
        public void CanCreateAmqpPropertiesWithContentEncoding()
        {
            IMessage message = A.Fake<IMessage>();
            const string headerValue = "utf-8";
            A.CallTo(() => message.Headers)
                .Returns(new Dictionary<string, string> { { AmqpPropertyBuilder.ContentEncoding, headerValue } });

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.Build(message);

            properties.ContentEncoding.Should().Be(headerValue);

            properties.Should().NotBeNull(because: "builder can build content encoding from message headers");
        }

        [Test(Description = "Can create amqp properties with content type.")]
        public void CanCreateAmqpPropertiesWithContentType()
        {
            IMessage message = A.Fake<IMessage>();
            const string headerValue = "text/plain";
            A.CallTo(() => message.Headers)
                .Returns(new Dictionary<string, string> { { AmqpPropertyBuilder.ContentType, headerValue } });

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.Build(message);

            properties.ContentType.Should().Be(headerValue);

            properties.Should().NotBeNull(because: "builder can build content type from message headers");
        }

        [Test(Description = "Can create amqp properties with correlation id.")]
        public void CanCreateAmqpPropertiesWithCorrelationId()
        {
            IMessage message = A.Fake<IMessage>();
            const string headerValue = "100500";
            A.CallTo(() => message.Headers)
                .Returns(new Dictionary<string, string> { { AmqpPropertyBuilder.CorrelationId, headerValue } });

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.Build(message);

            properties.CorrelationId.Should().Be(headerValue);

            properties.Should().NotBeNull(because: "builder can build correlation id from message headers");
        }

        [Test(Description = "Can create amqp properties with message id.")]
        public void CanCreateAmqpPropertiesWithMessageId()
        {
            IMessage message = A.Fake<IMessage>();
            const string headerValue = "100500";
            A.CallTo(() => message.Headers)
                .Returns(new Dictionary<string, string> { { AmqpPropertyBuilder.MessageId, headerValue } });

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.Build(message);

            properties.MessageId.Should().Be(headerValue);

            properties.Should().NotBeNull(because: "builder can build message id from message headers");
        }

        [Test(Description = "Can create amqp properties with persistent.")]
        public void CanCreateAmqpPropertiesWithPersistent()
        {
            IMessage message = A.Fake<IMessage>();
            const string headerValue = "true";
            A.CallTo(() => message.Headers)
                .Returns(new Dictionary<string, string> { { AmqpPropertyBuilder.Persistent, headerValue } });

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.Build(message);

            properties.Persistent.Should().Be(bool.Parse(headerValue));

            properties.Should().NotBeNull(because: "builder can build persistent from message headers");
        }

        [Test(Description = "Can create amqp properties with replyto.")]
        public void CanCreateAmqpPropertiesWithReplyTo()
        {
            IMessage message = A.Fake<IMessage>();
            const string headerValue = "some address";
            A.CallTo(() => message.Headers)
                .Returns(new Dictionary<string, string> { { AmqpPropertyBuilder.ReplyTo, headerValue } });

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.Build(message);

            properties.ReplyTo.Should().Be(headerValue);

            properties.Should().NotBeNull(because: "builder can build replyto from message headers");
        }

        [Test(Description = "Can create amqp properties with headers which not contain preset headers.")]
        public void CanCreateAmqpPropertiesWithHeaderNotContainPresetHeaders()
        {
            IMessage message = A.Fake<IMessage>();
            var customHeaders = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
            var allHeaders = new Dictionary<string, string>(customHeaders);
            allHeaders.Add(AmqpPropertyBuilder.ReplyTo, "some-address");
            allHeaders.Add(AmqpPropertyBuilder.ContentType, "some-address");

            A.CallTo(() => message.Headers).Returns(allHeaders);

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.Build(message);

            properties.Headers.Should().BeEquivalentTo(customHeaders, because: "preset headers should be set to properties");
        }
    }
}
