using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Tests
{
    [TestFixture]
    public class MessageBufferFactoryFixture
    {
        [Test(Description = "Can create a message buffer with default capacity.")]
        public void CanCreateWithDefaultCapacity()
        {
            const int Capacity = 15;

            var sut = new MessageBufferFactory(Capacity);

            IMessageBuffer result = sut.Create(null);

            result.Should().NotBeNull(because: "the factory should create a message action");
        }

        [Test(Description = "Can create a message buffer with capacity.")]
        public void CanCreateWithCapacity()
        {
            const int Capacity = 15;

            var sut = new MessageBufferFactory(Capacity);

            IMessageBuffer result = sut.Create(20);

            result.Should().NotBeNull(because: "the factory should create a message action");
        }
    }
}
