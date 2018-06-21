using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Kontur.Rabbitmq.Tests
{
    [TestFixture]
    internal class PublishingTagFixture
    {
        [Test]
        public void CanGetId()
        {
            string id = "_id_";
            var sut = new PublishingTag(id, (s) => { });

            sut.Id.Should().BeEquivalentTo("_id_", because: "Id shoud be set by constructor.");
        }

        [Test]
        public void CanDispose()
        {
            var action = A.Fake<Action<string>>();

            var sut = new PublishingTag("_id_", action);
            sut.Dispose();

            A.CallTo(() => action.Invoke(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
