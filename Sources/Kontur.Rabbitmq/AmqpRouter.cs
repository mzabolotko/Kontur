using System;
using System.Collections.Concurrent;

namespace Kontur.Rabbitmq
{
    internal class AmqpRouter : IAmqpRouter
    {
        private readonly ConcurrentDictionary<Type, Tuple<Func<IMessage, string>, Func<IMessage, string>>> resolvers;

        public AmqpRouter()
        {
            this.resolvers = new ConcurrentDictionary<Type, Tuple<Func<IMessage, string>, Func<IMessage, string>>>();
        }

        public string GetExchange(IMessage message)
        {
            if (this.resolvers.TryGetValue(message.RouteKey, out Tuple<Func<IMessage, string>, Func<IMessage, string>> resolver))
            {
                return resolver.Item1(message);
            }

            return null;
        }

        public string GetRoutingKey(IMessage message)
        {
            if (this.resolvers.TryGetValue(message.RouteKey, out Tuple<Func<IMessage, string>, Func<IMessage, string>> resolver))
            {
                return resolver.Item2(message);
            }

            return null;
        }

        public void Register<T>(Func<IMessage, string> exchangeResolve, Func<IMessage, string> routingKeyResolve)
        {
            this.resolvers.AddOrUpdate(
                typeof(T), 
                Tuple.Create(exchangeResolve, routingKeyResolve), 
                (type, add) => Tuple.Create(exchangeResolve, routingKeyResolve));
        }
    }
}
