using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Tests
{
    [TestFixture]
    public class SubscriberFactoryFixture
    {
        [Test(Description = "Can create subscriber")]
        public void CanCreate()
        {
            IMessageActionFactory messageActionFactory = A.Fake<IMessageActionFactory>();
            ILogServiceProvider logServiceProvider = A.Fake<ILogServiceProvider>();

            var sut = new SubscriberFactory(messageActionFactory, logServiceProvider);

            ISubscriber result = sut.Create<string>(m => {});

            result.Should().NotBeNull(because: "factory should create a new subscriber");
        }
    }
}
