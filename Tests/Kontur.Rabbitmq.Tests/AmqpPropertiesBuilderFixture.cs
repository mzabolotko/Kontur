using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using RabbitMQ.Client;
using System.Collections.Generic;

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

            IAmqpProperties properties = sut.BuildPropertiesFromHeaders(message.Headers);

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

            IAmqpProperties properties = sut.BuildPropertiesFromHeaders(message.Headers);

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

            IAmqpProperties properties = sut.BuildPropertiesFromHeaders(message.Headers);

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

            IAmqpProperties properties = sut.BuildPropertiesFromHeaders(message.Headers);

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

            IAmqpProperties properties = sut.BuildPropertiesFromHeaders(message.Headers);

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

            IAmqpProperties properties = sut.BuildPropertiesFromHeaders(message.Headers);

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

            IAmqpProperties properties = sut.BuildPropertiesFromHeaders(message.Headers);

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

            IAmqpProperties properties = sut.BuildPropertiesFromHeaders(message.Headers);

            properties.Headers.Should().BeEquivalentTo(customHeaders, because: "preset headers should be set to properties");
        }

        [Test(Description = "Can create empty amqp properties from BasicProperties")]
        public void CanCreateEmptyAmqpPropertiesFromBasicProperites()
        {
            IBasicProperties basicProperties = A.Fake<IBasicProperties>();
            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.BuildPropertiesFromProperties(basicProperties);

            properties.Should().NotBeNull(because: "Builder can build empty properties");
        }

        [Test(Description = "Can create amqp properties with content endcoding from BasicProperties")]
        public void CanCreateAmqpPropertiesWithContentEncodingFromBasicProperites()
        {
            IBasicProperties basicProperties = A.Fake<IBasicProperties>();
            const string headerValue = "utf-8";
            A.CallTo(() => basicProperties.ContentEncoding).Returns(headerValue);

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.BuildPropertiesFromProperties(basicProperties);

            properties.ContentEncoding.Should().Be(headerValue);

            properties.Should().NotBeNull(because: "builder can build content encoding from BasicProperties");
        }

        [Test(Description = "Can create amqp properties with content type from BasicProperties")]
        public void CanCreateAmqpPropertiesWithContentTypeFromBasicProperites()
        {
            IBasicProperties basicProperties = A.Fake<IBasicProperties>();
            const string headerValue = "text/plain";
            A.CallTo(() => basicProperties.ContentType).Returns(headerValue);

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.BuildPropertiesFromProperties(basicProperties);

            properties.ContentType.Should().Be(headerValue);

            properties.Should().NotBeNull(because: "builder can build content type from BasicProperties");
        }

        [Test(Description = "Can create amqp properties with correlation id from BasicProperties")]
        public void CanCreateAmqpPropertiesWithCorrelationIdFromBasicProperties()
        {
            IBasicProperties basicProperties = A.Fake<IBasicProperties>();
            const string headerValue = "100500";
            A.CallTo(() => basicProperties.CorrelationId).Returns(headerValue);

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.BuildPropertiesFromProperties(basicProperties);

            properties.CorrelationId.Should().Be(headerValue);

            properties.Should().NotBeNull(because: "builder can build correlation id from BasicProperties");
        }

        [Test(Description = "Can create amqp properties with message id from BasicProperties")]
        public void CanCreateAmqpPropertiesWithMessageIdFromBasicProperties()
        {
            IBasicProperties basicProperties = A.Fake<IBasicProperties>();
            const string headerValue = "100500";
            A.CallTo(() => basicProperties.MessageId).Returns(headerValue);

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.BuildPropertiesFromProperties(basicProperties);

            properties.MessageId.Should().Be(headerValue);

            properties.Should().NotBeNull(because: "builder can build message id from BasicProperties");
        }

        [Test(Description = "Can create amqp properties with persistent from BasicProperties")]
        public void CanCreateAmqpPropertiesWithPersistentFromBasicProperties()
        {
            IBasicProperties basicProperties = A.Fake<IBasicProperties>();
            const string headerValue = "true";
            A.CallTo(() => basicProperties.Persistent).Returns(bool.Parse(headerValue));

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.BuildPropertiesFromProperties(basicProperties);

            properties.Persistent.Should().Be(bool.Parse(headerValue));

            properties.Should().NotBeNull(because: "builder can build persistent from BasicProperties");
        }

        [Test(Description = "Can create amqp properties with replyto from BasicProperties")]
        public void CanCreateAmqpPropertiesWithReplyToFromBasicProperties()
        {
            IBasicProperties basicProperties = A.Fake<IBasicProperties>();
            const string headerValue = "some address";
            A.CallTo(() => basicProperties.ReplyTo).Returns(headerValue);

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IAmqpProperties properties = sut.BuildPropertiesFromProperties(basicProperties);

            properties.ReplyTo.Should().Be(headerValue);

            properties.Should().NotBeNull(because: "builder can build replyto from BasicProperties");
        }

        [Test(Description = "Can create empty headers from amqp properties")]
        public void CanCreateEmptyHeadersFromAmqpProperties()
        {
            IAmqpProperties properties = A.Fake<IAmqpProperties>();
            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IDictionary<string, string> headers = sut.BuildHeadersFromProperties(properties);

            headers.Should().NotBeNull(because: "Builder can build empty headers");
        }

        [Test(Description = "Can create headers with content endcoding from AmqpProperties")]
        public void CanCreateHeadersWithContentEncodingFromAmqpProperties()
        {
            IAmqpProperties properties = A.Fake<IAmqpProperties>();
            const string headerValue = "utf-8";
            A.CallTo(() => properties.ContentEncoding).Returns(headerValue);

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IDictionary<string, string> headers = sut.BuildHeadersFromProperties(properties);

            headers.Should().Contain(new KeyValuePair<string, string>(AmqpPropertyBuilder.ContentEncoding, headerValue));

            headers.Should().NotBeNull(because: "builder can build content encoding from AmqpProperties");
        }

        [Test(Description = "Can create headers with content type from AmqpProperties")]
        public void CanCreateHeadersWithContentTypeFromAmqpProperties()
        {
            IAmqpProperties properties = A.Fake<IAmqpProperties>();
            const string headerValue = "text/plain";
            A.CallTo(() => properties.ContentType).Returns(headerValue);

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IDictionary<string, string> headers = sut.BuildHeadersFromProperties(properties);

            headers.Should().Contain(new KeyValuePair<string, string>(AmqpPropertyBuilder.ContentType, headerValue));

            headers.Should().NotBeNull(because: "builder can build content type from AmqpProperties");
        }

        [Test(Description = "Can create headers with correlation id from AmqpProperties")]
        public void CanCreateHeadersWithCorrelationIdFromAmqpProperties()
        {
            IAmqpProperties properties = A.Fake<IAmqpProperties>();
            const string headerValue = "100500";
            A.CallTo(() => properties.CorrelationId).Returns(headerValue);

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IDictionary<string, string> headers = sut.BuildHeadersFromProperties(properties);

            headers.Should().Contain(new KeyValuePair<string, string>(AmqpPropertyBuilder.CorrelationId, headerValue));

            headers.Should().NotBeNull(because: "builder can build correlation id from AmqpProperties");
        }

        [Test(Description = "Can create headers with message id from AmqpProperties")]
        public void CanCreateHeadersWithMessageIdFromAmqpProperties()
        {
            IAmqpProperties properties = A.Fake<IAmqpProperties>();
            const string headerValue = "100500";
            A.CallTo(() => properties.MessageId).Returns(headerValue);

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IDictionary<string, string> headers = sut.BuildHeadersFromProperties(properties);

            headers.Should().Contain(new KeyValuePair<string, string>(AmqpPropertyBuilder.MessageId, headerValue));

            headers.Should().NotBeNull(because: "builder can build correlation id from AmqpProperties");
        }

        [Test(Description = "Can create headers with persistent from AmqpProperties")]
        public void CanCreateHeadersWithPersistentFromAmqpProperties()
        {
            IAmqpProperties properties = A.Fake<IAmqpProperties>();
            const string headerValue = "True";
            A.CallTo(() => properties.Persistent).Returns(bool.Parse(headerValue));

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IDictionary<string, string> headers = sut.BuildHeadersFromProperties(properties);

            headers.Should().Contain(new KeyValuePair<string, string>(AmqpPropertyBuilder.Persistent, headerValue));

            headers.Should().NotBeNull(because: "builder can build correlation id from AmqpProperties");
        }

        [Test(Description = "Can create headers with replyto from AmqpProperties")]
        public void CanCreateHeadersWithReplyToFromAmqpProperties()
        {
            IAmqpProperties properties = A.Fake<IAmqpProperties>();
            const string headerValue = "some address";
            A.CallTo(() => properties.ReplyTo).Returns(headerValue);

            AmqpPropertyBuilder sut = new AmqpPropertyBuilder();

            IDictionary<string, string> headers = sut.BuildHeadersFromProperties(properties);

            headers.Should().Contain(new KeyValuePair<string, string>(AmqpPropertyBuilder.ReplyTo, headerValue));

            headers.Should().NotBeNull(because: "builder can build correlation id from AmqpProperties");
        }
    }
}
