using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kontur
{
    public class Bus : IPublisherRegistry, ISubscriptionRegistry
    {
        private readonly ILogServiceProvider logServiceProvider;
        private readonly ILogService logService;
        private readonly IInbox inbox;
        private readonly IOutbox outbox;
        private readonly IExchange exchange;

        public Bus(IInbox inbox, IOutbox outbox, IExchange exchange, ILogServiceProvider logServiceProvider)
        {
            this.inbox = inbox;
            this.outbox = outbox;
            this.exchange = exchange;
            this.logServiceProvider = logServiceProvider;
            this.logService = this.logServiceProvider.GetLogServiceOf(typeof(Bus));
            this.logService.Info("Started the bus instance.");
        }

        public ISubscriptionTag Subscribe<T>(Action<Message<T>> subscriber, int queueCapacity = 1)
        {
            IMessageBuffer queue = this.outbox.CreateSubscriberQueue<T>();
            this.exchange.BindSubscriberQueue<T>(this.inbox, queue.AsTarget);
            ISubscriptionTag subscriptionTag = this.outbox.Subscribe<T>(queue, subscriber);
            return subscriptionTag;
        }

        public ISubscriptionTag Subscribe<T>(ISubscriber subscriber, int queueCapacity = 1)
        {
            IMessageBuffer queue = this.outbox.CreateSubscriberQueue<T>();
            this.exchange.BindSubscriberQueue<T>(this.inbox, queue.AsTarget);
            ISubscriptionTag subscriptionTag = this.outbox.Subscribe<T>(queue, subscriber);
            return subscriptionTag;
        }

        public bool IsSubscribed(ISubscriptionTag tag)
        {
            return this.outbox.IsSubscribed(tag);
        }

        public void Unsubscribe(ISubscriptionTag tag)
        {
            this.outbox.Unsubscribe(tag);
        }

        public Task<bool> EmitAsync<T>(T payload, IDictionary<string, string> headers)
        {
            var tcs = new TaskCompletionSource<bool>();
            var message = new Message<T>(payload, headers, tcs);

            return this.inbox.EmitAsync<T>(message);
        }

        public int GetInboxMessageCount<T>()
        {
            return this.inbox.GetInboxMessageCount<T>();
        }

        public IPublishingTag RegisterPublisher<T>(IPublisher publisher)
        {
            this.logService.Info("Registering a publisher of {0}.", typeof(T));
            IMessageBuffer inboxQueue = this.exchange.BindPublisher<T>(inbox);
            IPublishingTag tag = this.inbox.RegisterPublisher<T>(publisher, inboxQueue);

            return tag;
        }

        public bool IsRegistered(IPublishingTag tag)
        {
            return this.inbox.IsRegistered(tag);
        }

        public void Unregister(IPublishingTag tag)
        {
            this.inbox.Unregister(tag);
        }
    }
}
