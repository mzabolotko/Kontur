using System;

namespace Kontur.Rabbitmq
{
    internal interface IAmqpRouter
    {
        void Register<T>(Func<IMessage, string> exchangeResolve, Func<IMessage, string> routingKeyResolve);

        string GetExchange(IMessage message);
        string GetRoutingKey(IMessage message);
    }
}