using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Tests
{
    [TestFixture]
    public class MessageActionFactoryFixture
    {
        [Test(Description = "Can create a message action with an action")]
        public void CanCreateWithAction()
        {
            var sut = new MessageActionFactory();

            IMessageAction result = sut.Create(m => {});

            result.Should().NotBeNull(because: "the factory should create a message action");
        }

        [Test(Description = "Can create a message action with an func")]
        public void CanCreateWithFunc()
        {
            var sut = new MessageActionFactory();

            IMessageAction result = sut.Create(m => { return Task.CompletedTask; });

            result.Should().NotBeNull(because: "the factory should create a message action");
        }
    }
}
