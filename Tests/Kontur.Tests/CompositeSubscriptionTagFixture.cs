using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Kontur.Tests
{
    [TestFixture]
    internal class CompositeSubscriptionTagFixture
    {
        [Test]
        public void CanGetId()
        {
            string id = "_id_";
            var sut = new CompositeSubscriptionTag(id, new List<ISubscriptionTag>());

            sut.Id.Should().BeEquivalentTo("_id_", because: "Id shoud be set by constructor.");
        }

        [Test]
        public void CanDispose()
        {
            var tag1 = A.Fake<ISubscriptionTag>();
            var tag2 = A.Fake<ISubscriptionTag>();

            var sut = new CompositeSubscriptionTag("_id_", new List<ISubscriptionTag> { tag1, tag2 });
            sut.Dispose();

            A.CallTo(() => tag1.Dispose()).MustHaveHappenedOnceExactly();
            A.CallTo(() => tag2.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}
