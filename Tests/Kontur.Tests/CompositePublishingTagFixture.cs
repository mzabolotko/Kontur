using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Kontur.Tests
{
    [TestFixture]
    internal class CompositePublishingTagFixture
    {
        [Test]
        public void CanGetId()
        {
            string id = "_id_";
            var sut = new CompositePublishingTag(id, new List<IPublishingTag>());

            sut.Id.Should().BeEquivalentTo("_id_", because: "Id shoud be set by constructor.");
        }

        [Test]
        public void CanDispose()
        {
            var tag1 = A.Fake<IPublishingTag>();
            var tag2 = A.Fake<IPublishingTag>();

            var sut = new CompositePublishingTag("_id_", new List<IPublishingTag> { tag1, tag2 });
            sut.Dispose();

            A.CallTo(() => tag1.Dispose()).MustHaveHappenedOnceExactly();
            A.CallTo(() => tag2.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}
