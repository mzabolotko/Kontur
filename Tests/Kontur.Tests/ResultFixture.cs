using System.Runtime.ExceptionServices;
using FluentAssertions;
using NUnit.Framework;

namespace Kontur.Tests
{
    [TestFixture]
    internal class ResultFixture
    {
        [Test(Description = "Can create success result.")]
        public void CanCreateSuccessResult()
        {
            var sut = new Result<int, ExceptionDispatchInfo>(10);

            sut.Success.Should().BeTrue();
        }

        [Test(Description = "Can create failure result.")]
        public void CanCreateFailureResult()
        {

            var sut = new Result<int, ExceptionDispatchInfo>((ExceptionDispatchInfo)null);

            sut.Success.Should().BeFalse();
        }
    }
}
