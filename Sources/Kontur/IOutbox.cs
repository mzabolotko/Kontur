using System;

namespace Kontur
{
    public interface IOutbox
    {
        IMessageBuffer CreateSubscriberQueue<T>(int queueCapacity = 1);

        ISubscriptionTag Subscribe<T>(IMessageBuffer subscriberQueue, Action<Message<T>> subscriber);

        ISubscriptionTag Subscribe<T>(IMessageBuffer subscrberQueuey, ISubscriber subscriber);

        bool IsSubscribed(ISubscriptionTag tag);

        void Unsubscribe(ISubscriptionTag tag);
    }
}
