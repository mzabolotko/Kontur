using NUnit.Framework;
using RabbitMQ.Client;
using System.Collections.Generic;

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
                sut.EmitAsync("hello, world!", new Dictionary<string, string> { { "content-type", "text/plain" } }).Wait();
                System.Threading.Thread.Sleep(50000);
            }
        }
    }
}
