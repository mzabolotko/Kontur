using NUnit.Framework;
using System.Collections.Generic;

namespace Kontur.Rabbitmq.IntegrationTests
{
    [TestFixture]
    [Explicit("Need the started rabbitmq broker.")]
    public class PublishingToAmqpFixture
    {
        [Test]
        public void CanPublishToAmqp()
        {
            var sut = new Bus();

            using (var subsciption = sut.ToRabbitMq(cfg => 
            {
                cfg.RouteTo<string>("test", string.Empty);

                return cfg;
            }))
            {
                Assert.DoesNotThrowAsync(
                    () => sut.EmitAsync("hello, world!", new Dictionary<string, string> { { "content-type", "text/plain" } }),
                    "Message should be pulished");
            }
        }
    }
}
