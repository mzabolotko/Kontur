using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FakeItEasy;
using FluentAssertions;
using Kontur.Rabbitmq.Tests.Plumbing;
using NUnit.Framework;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    class AmqpSerializerFactoryFixture
    {
        [Test]
        public void CreateAmqpSerializerWithDefaultSerializerIfMessageContentTypeWasNotFilled()
        {
            var serializers = GetAmqpSerializers();
            var message = A.Fake<IMessage>();
            var sut = new AmqpSerializerFactory(new ReadOnlyDictionary<string, IAmqpSerializer>(serializers));

            IAmqpSerializer serializer = sut.CreateSerializer(message);

            serializer.Should().BeOfType<SimpleSerializer>(because: "factory should return default serializer");
        }

        [Test]
        public void CreateAmqpSerializerWithCustomSerializerDeterminedByContentType()
        {
            var serializers = GetAmqpSerializers();
            var message = A.Fake<IMessage>();
            A.CallTo(() => message.Headers).Returns(new Dictionary<string, string>
            {
                { "content-type", "application/json" }
            });
            var sut = new AmqpSerializerFactory(new ReadOnlyDictionary<string, IAmqpSerializer>(serializers));

            IAmqpSerializer serializer = sut.CreateSerializer(message);

            serializer.Should().BeOfType<JsonSerializer>(because: "factory should not return default serializer");
        }

        [Test]
        public void DefaultSerializerShouldBeChooIfSerializerByContentTypeWasNowFound()
        {
            var serializers = GetAmqpSerializers();
            var message = A.Fake<IMessage>();
            A.CallTo(() => message.Headers).Returns(new Dictionary<string, string>
            {
                { "content-type", "application/x-binary" }
            });
            var sut = new AmqpSerializerFactory(new ReadOnlyDictionary<string, IAmqpSerializer>(serializers));

            IAmqpSerializer serializer = sut.CreateSerializer(message);

            serializer.Should().BeOfType<SimpleSerializer>(because: "factory should return default serializer");
        }

        [Test]
        public void ThrowsIfNoSerializersWasProvided()
        {
            var serializers = new Dictionary<string, IAmqpSerializer>();
            var message = A.Fake<IMessage>();

            Assert.Throws<ArgumentException>(() => new AmqpSerializerFactory(new ReadOnlyDictionary<string, IAmqpSerializer>(serializers)), "should be provided at least one serializer");
        }

        [Test]
        public void ThrowsIfSerializersAreNull()
        {
            ReadOnlyDictionary<string, IAmqpSerializer> serializers = null;
            var message = A.Fake<IMessage>();

            Assert.Throws<ArgumentNullException>(() => new AmqpSerializerFactory(serializers), "serializers should not be null");
        }

        private static Dictionary<string, IAmqpSerializer> GetAmqpSerializers()
        {
            return new Dictionary<string, IAmqpSerializer>
            {
                {"plain/text", new SimpleSerializer()},
                {"application/json", new JsonSerializer()}
            };
        }
    }
}
