using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Kontur.Tests
{
    [TestFixture]
    class SubscriptionTagFixture
    {
        [Test]
        public void CanGetId()
        {
            string id = "_id_";
            var sut = new SubscriptionTag(id, null);

            sut.Id.Should().BeEquivalentTo("_id_", because: "Id shoud be set by constructor.");
        }

        [Test]
        public void CanDispose()
        {
            var disposable = A.Fake<IDisposable>();

            var sut = new SubscriptionTag("_id_", disposable);
            sut.Dispose();

            A.CallTo(() => disposable.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}
