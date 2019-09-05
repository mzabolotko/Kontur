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
            var messageBufferFactory = new MessageBufferFactory(10);
            var messageActionFactory = new MessageActionFactory();
            var nlogServiceProvider = new NUnitLogProvider();

            var inbox = new Inbox(messageBufferFactory, messageActionFactory, nlogServiceProvider);
            var outbox = new Outbox(messageBufferFactory, messageActionFactory, nlogServiceProvider);
            var exchange = new Exchange(nlogServiceProvider);
            var sut = new Bus(inbox, outbox, exchange, nlogServiceProvider);

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
