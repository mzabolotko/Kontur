using RabbitMQ.Client;
using System;

namespace Kontur.Rabbitmq
{
    internal class AmqpConnectionFactory : IAmqpConnectionFactory
    {
        private readonly ConnectionFactory factory;

        public AmqpConnectionFactory(Uri uri)
        {
            this.factory = new ConnectionFactory();
            this.factory.Uri = uri;
        }

        public IConnection CreateConnection()
        {
            return this.factory.CreateConnection();
        }
    }
}
