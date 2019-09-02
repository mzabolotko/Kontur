using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kontur
{
    public class Bus : IPublisherRegistry, ISubscriptionRegistry
    {
        private readonly ConcurrentDictionary<string, IPublishingTag> publishers;
        private readonly ILogServiceProvider logServiceProvider;
        private readonly ILogService logService;
        private readonly IInbox inbox;
        private readonly IOutbox outbox;
        private readonly IExchange exchange;
        private readonly MessageBufferFactory messageBufferFactory;
        private readonly MessageActionFactory messageActionFactory;

        public Bus(IInbox inbox = null, IOutbox outbox = null, IExchange exchange = null, ILogServiceProvider logServiceProvider = null)
        {
            this.messageBufferFactory = new MessageBufferFactory(10);
            this.messageActionFactory = new MessageActionFactory();
            this.inbox = inbox ?? new Inbox(this.messageBufferFactory, this.messageActionFactory, logServiceProvider);
            this.outbox = outbox ?? new Outbox(this.messageBufferFactory, this.messageActionFactory, logServiceProvider);
            this.exchange = exchange ?? new Exchange(logServiceProvider);
            this.publishers = new ConcurrentDictionary<string, IPublishingTag>();
            this.logServiceProvider = logServiceProvider ?? new NullLogServiceProvider();
            this.logService = this.logServiceProvider.GetLogServiceOf(typeof(Bus));

            this.logService.Info("Started the bus instance.");
        }

        public ISubscriptionTag Subscribe<T>(Action<Message<T>> subscriber, int queueCapacity = 1)
        {
            IMessageBuffer queue = this.outbox.CreateSubscriberQueue<T>();
            this.exchange.BindSubscriber<T>(this.inbox, queue.AsTarget);
            ISubscriptionTag subscriptionTag = this.outbox.Subscribe<T>(queue, subscriber);
            return subscriptionTag;
        }

        public ISubscriptionTag Subscribe<T>(ISubscriber subscriber, int queueCapacity = 1)
        {
            IMessageBuffer queue = this.outbox.CreateSubscriberQueue<T>();
            this.exchange.BindSubscriber<T>(this.inbox, queue.AsTarget);
            ISubscriptionTag subscriptionTag = this.outbox.Subscribe<T>(queue, subscriber);
            return subscriptionTag;
        }

        public Task EmitAsync<T>(T payload, IDictionary<string, string> headers)
        {
            var tcs = new TaskCompletionSource<bool>();
            var message = new Message<T>(payload, headers, tcs);

            return this.inbox.EmitAsync<T>(message);
        }

        public int GetInboxMessageCount<T>()
        {
            return this.inbox.GetInboxMessageCount<T>();
        }

        public bool IsSubscribed(ISubscriptionTag tag)
        {
            return this.outbox.IsSubscribed(tag);
        }

        public void Unsubscribe(ISubscriptionTag tag)
        {
            this.outbox.Unsubscribe(tag);
        }

        public bool IsRegistered(IPublishingTag tag)
        {
            return publishers.ContainsKey(tag.Id);
        }

        public void Unregister(IPublishingTag tag)
        {
            this.logService.Info("Unregistering a publisher.");
            tag.Dispose();
            this.publishers.TryRemove(tag.Id, out var publisher);
        }

        public IPublishingTag RegisterPublisher<T>(IPublisher publisher)
        {
            this.logService.Info("Registering a publisher of {0}.", typeof(T));
            IMessageBuffer messageBuffer = this.exchange.BindPublisher<T>(inbox);

            IPublishingTag tag = publisher.LinkTo(messageBuffer.AsTarget);
            tag = this.publishers.AddOrUpdate(tag.Id, tag, (id, t) => t);

            return tag;
        }
    }
}
