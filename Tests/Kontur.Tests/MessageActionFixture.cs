using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Tests
{
    [TestFixture]
    public class MessageActionFixture
    {
        [Test(Description = "Can get a message action as a target")]
        public void CanGetAsTarget()
        {
            ExecutionDataflowBlockOptions defaultDispatcherOptions = new ExecutionDataflowBlockOptions();
            var sut = new MessageAction(m => { return Task.CompletedTask; }, defaultDispatcherOptions);

            sut.AsTarget.Should().NotBeNull(because: "the created message action can be used as a target");
        }
    }
}
