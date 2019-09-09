using System;
using System.Collections.Concurrent;

namespace Kontur
{
    public class Outbox : IOutbox
    {
        private readonly ILogService logService;
        private readonly IMessageBufferFactory messageBufferFactory;
        private readonly ISubscriberFactory subscriberFactory;
        private readonly ConcurrentDictionary<string, ISubscriptionTag> subscribers;

        public Outbox(IMessageBufferFactory messageBufferFactory, ISubscriberFactory subscriberFactory, ILogServiceProvider logServiceProvider)
        {
            this.messageBufferFactory = messageBufferFactory;
            this.subscriberFactory = subscriberFactory;
            this.subscribers = new ConcurrentDictionary<string, ISubscriptionTag>();
            this.logService = logServiceProvider.GetLogServiceOf(typeof(Outbox));
            this.logService.Info("Created an outbox.");
        }

        public IMessageBuffer CreateSubscriberQueue<T>(int queueCapacity = 1)
        {
            this.logService.Info("Creating subscription to {0} with the queue capacity equals: {1}.", typeof(T), queueCapacity);

            IMessageBuffer workerQueue = this.messageBufferFactory.Create(queueCapacity);
            return workerQueue;
        }

        public ISubscriptionTag Subscribe<T>(IMessageBuffer workerQueue, Action<Message<T>> action)
        {
            ISubscriber subscriber = this.subscriberFactory.Create(action);
            return this.Subscribe<T>(workerQueue, subscriber);
        }

        public ISubscriptionTag Subscribe<T>(IMessageBuffer workerQueue, ISubscriber subscriber)
        {
            ISubscriptionTag subscriberTag = subscriber.SubscribeTo(workerQueue.AsSource);

            subscribers.AddOrUpdate(subscriberTag.Id, subscriberTag, (o, n) => n);

            return subscriberTag;
        }

        public bool IsSubscribed(ISubscriptionTag tag)
        {
            return subscribers.ContainsKey(tag.Id);
        }

        public void Unsubscribe(ISubscriptionTag tag)
        {
            if (this.subscribers.TryRemove(tag.Id, out var subscriber))
            {
                this.logService.Info("Unsubscribing a subscription.");
                tag.Dispose();
            }
        }
    }
}
