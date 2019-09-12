using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Tests
{
    [TestFixture]
    public class MessageBufferFixture
    {
        [Test(Description = "Can get a message buffer as a target")]
        public void CanGetAsTarget()
        {
            DataflowBlockOptions options = new DataflowBlockOptions();
            var sut = new MessageBuffer(options);

            sut.AsTarget.Should().NotBeNull(because: "the created message buffer can be used as a target");
        }

        [Test(Description = "Can get a message buffer as a source")]
        public void CanGetAsSource()
        {
            DataflowBlockOptions options = new DataflowBlockOptions();
            var sut = new MessageBuffer(options);

            sut.AsSource.Should().NotBeNull(because: "the created message buffer can be used as a source");
        }

        [Test(Description = "Can get a count message buffer")]
        public void CanGetCount()
        {
            DataflowBlockOptions options = new DataflowBlockOptions();
            var sut = new MessageBuffer(options);

            sut.Count.Should().Be(0, because: "the created message buffer is empty");
        }

    }
}
