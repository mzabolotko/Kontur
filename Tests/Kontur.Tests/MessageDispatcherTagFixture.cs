using FakeItEasy;
using NUnit.Framework;
using System;

namespace Kontur.Tests
{
    [TestFixture]
    internal class MessageDispatcherTagFixture
    {
        [Test]
        public void CanCallUnsubcribeActionWhenDispose()
        {
            Action<string> unsubscribeAction = A.Fake<Action<string>>();

            var sut = new MessageDispatcherTag("_tag_", unsubscribeAction);

            sut.Dispose();

            A.CallTo(
                () => unsubscribeAction.Invoke(A<string>.That.IsEqualTo("_tag_")))
                .MustHaveHappenedOnceExactly();
        }
    }
}
