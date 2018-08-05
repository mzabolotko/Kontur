using Newtonsoft.Json;
using NUnit.Framework;
using System.Threading;

namespace Kontur.Rabbitmq.IntegrationTests
{
    [TestFixture]
    [Explicit("Need the started rabbitmq broker.")]
    public class ConsumingFromAmqpFixture
    {
        [Test]
        public void CanConsumeFromAmqp()
        {
            var sut = new Bus();
            var manualResetEvent = new ManualResetEvent(false);

            using (var publishing = sut.FromRabbitMq(cfg =>
            {
                cfg.ReactOn<Message>("test");
                cfg.WithDeserializerFactory(new DeserializerFactory());

                return cfg;
            }))
            {
                sut.Subscribe<Message>(message =>
                {
                    System.Console.WriteLine(message.Payload.Id);
                    manualResetEvent.Set();
                });
                manualResetEvent.WaitOne();
            };
        }
    }

    internal class Message
    {
        public string Id { get; set; }
    }

    internal class JsonNetDeserializer : IAmqpDeserializer
    {
        public T Deserialize<T>(AmqpMessage amqpMessage)
        {
            string payload = System.Text.Encoding.UTF8.GetString(amqpMessage.Payload);
            return JsonConvert.DeserializeObject<T>(payload);
        }
    }

    internal class DeserializerFactory : IAmqpDeserializerFactory
    {
        public IAmqpDeserializer CreateDeserializer(AmqpMessage amqpMessage)
        {
            return new JsonNetDeserializer();
        }
    }
}
