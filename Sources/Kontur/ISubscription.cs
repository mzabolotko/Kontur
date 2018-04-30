using System;

namespace Kontur
{
    public interface ISubscriptionRegistry
    {
        bool IsSubscribed(ISubscriptionTag tag);
        ISubscriptionTag Subscribe<T>(Action<Message<T>> subscriber);
        ISubscriptionTag Subscribe<T>(ISubscriber subscriber);
        void Unsubscribe(ISubscriptionTag tag);
    }
}