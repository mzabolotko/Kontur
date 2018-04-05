using NUnit.Framework;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kontur.Rabbitmq.IntegrationTests
{
    [TestFixture]
    public class PublishingToAmqpFixture
    {
        [Test]
        public void CanPublishToAmqp()
        {
            var sut = new Bus();

            using (var subsciption = sut.WithRabbitMq<string>(cfg => 
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
