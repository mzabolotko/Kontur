using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Tests
{
    [TestFixture]
    public class MessageDispatcherFactoryFixture
    {
        [Test(Description = "Can create a message dispatcher")]
        public void CanCreateMessageDispatcher()
        {
            ILogServiceProvider logServiceProvider = A.Fake<ILogServiceProvider>();

            var sut = new MessageDispatcherFactory(logServiceProvider);

            IMessageDispatcher result = sut.Create();

            result.Should().NotBeNull(because: "the factory should create a new message dispatcher");
        }
    }
}
