using System;
using System.Collections.Concurrent;

namespace Kontur
{
    public class Outbox : IOutbox
    {
        private readonly ILogServiceProvider logServiceProvider;
        private readonly ILogService logService;
        private readonly IMessageBufferFactory messageBufferFactory;
        private readonly IMessageActionFactory messageActionFactory;
        private readonly ConcurrentDictionary<string, ISubscriptionTag> subscribers;

        public Outbox(IMessageBufferFactory messageBufferFactory, IMessageActionFactory messageActionFactory, ILogServiceProvider logServiceProvider = null)
        {
            this.messageBufferFactory = messageBufferFactory;
            this.messageActionFactory = messageActionFactory;
            this.subscribers = new ConcurrentDictionary<string, ISubscriptionTag>();
            this.logServiceProvider = logServiceProvider ?? new NullLogServiceProvider();
            this.logService = this.logServiceProvider.GetLogServiceOf(typeof(Outbox));
            this.logService.Info("Created an outbox.");
            this.logServiceProvider = logServiceProvider;
        }

        public IMessageBuffer CreateSubscriberQueue<T>(int queueCapacity = 1)
        {
            this.logService.Info("Creating subscription to {0} with the queue capacity equals: {1}.", typeof(T), queueCapacity);

            IMessageBuffer workerQueue = this.messageBufferFactory.Create(queueCapacity);
            return workerQueue;
        }

        public ISubscriptionTag Subscribe<T>(IMessageBuffer workerQueue, Action<Message<T>> subscriber)
        {
            var messageSubscriber = new MessageSubscriber<T>(subscriber, this.messageActionFactory, this.logServiceProvider);
            return this.Subscribe<T>(workerQueue, messageSubscriber);
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
            this.logService.Info("Unsubscribing a subscription.");
            tag.Dispose();
            this.subscribers.TryRemove(tag.Id, out var subscriber);
        }
    }
}
