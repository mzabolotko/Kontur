using System;

namespace Kontur
{
    public interface ISubscriptionRegistry
    {
        bool IsSubscribed(ISubscriptionTag tag);

        ISubscriptionTag Subscribe<T>(Action<Message<T>> subscriber, int queueCapacity = 1);

        ISubscriptionTag Subscribe<T>(ISubscriber subscriber, int queueCapacity = 1);

        void Unsubscribe(ISubscriptionTag tag);
    }
}