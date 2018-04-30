using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    internal class SubscribingTagFixture
    {
        [Test]
        public void CanGetId()
        {
            string id = "_id_";
            var sut = new SubscribingTag(id, () => { });

            sut.Id.Should().BeEquivalentTo("_id_", because: "Id shoud be set by constructor.");
        }

        [Test]
        public void CanDispose()
        {
            var action = A.Fake<Action>();

            var sut = new SubscribingTag("_id_", action);
            sut.Dispose();

            A.CallTo(() => action.Invoke()).MustHaveHappenedOnceExactly();
        }
    }
}
