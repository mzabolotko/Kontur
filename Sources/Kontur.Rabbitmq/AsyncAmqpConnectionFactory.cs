using RabbitMQ.Client;
using System;

namespace Kontur.Rabbitmq
{
    internal class AsyncAmqpConnectionFactory : IAmqpConnectionFactory
    {
        private readonly ConnectionFactory factory;

        public AsyncAmqpConnectionFactory(Uri uri)
        {
            this.factory = new ConnectionFactory();
            this.factory.Uri = uri;
            this.factory.DispatchConsumersAsync = true;
        }

        public IConnection CreateConnection()
        {
            return this.factory.CreateConnection();
        }
    }
}
