using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using MessageBuffer = System.Threading.Tasks.Dataflow.BufferBlock<Kontur.IMessage>;
using MessageAction = System.Threading.Tasks.Dataflow.ActionBlock<Kontur.IMessage>;

namespace Kontur
{
    public class Bus : IPublisherRegistry, ISubscriptionRegistry
    {
        private readonly ConcurrentDictionary<Type, MessageBuffer> inboxes =
            new ConcurrentDictionary<Type, MessageBuffer>();
        private readonly ConcurrentDictionary<Type, MessageDispatcher> dispatchers =
            new ConcurrentDictionary<Type, MessageDispatcher>();
        private readonly ConcurrentDictionary<string, ISubscriptionTag> subscribers =
            new ConcurrentDictionary<string, ISubscriptionTag>();
        private readonly ConcurrentDictionary<string, IPublishingTag> publishers =
            new ConcurrentDictionary<string, IPublishingTag>();

        private readonly ExecutionDataflowBlockOptions defaultDistpatcherOptions =
            new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 1
            };

        private readonly DataflowBlockOptions defaultInboxQueueOptions;
        private readonly ILogServiceProvider logServiceProvider;
        private readonly ILogService logService;

        public Bus(int inboxCapacity = 10, ILogServiceProvider logServiceProvider = null)
        {
            this.logServiceProvider = logServiceProvider ?? new NullLogServiceProvider();
            this.logService = this.logServiceProvider.GetLogServiceOf(typeof(Bus));
            this.defaultInboxQueueOptions = new DataflowBlockOptions
            {
                BoundedCapacity = inboxCapacity
            };
            this.logService.Info("Started the bus instance with the inbox capacity equals: {0}.", inboxCapacity);
        }

        public ISubscriptionTag Subscribe<T>(Action<Message<T>> subscriber, int queueCapacity = 1)
        {
            var messageSubscriber = new MessageSubscriber<T>(subscriber, this.logServiceProvider);
            return this.Subscribe<T>(messageSubscriber, queueCapacity);
        }

        public ISubscriptionTag Subscribe<T>(ISubscriber subscriber, int queueCapacity = 1)
        {
            this.logService.Info("Creating subscription to {0} with the queue capacity equals: {1}.", typeof(T), queueCapacity);
            DataflowBlockOptions queueOptions = new DataflowBlockOptions
            {
                BoundedCapacity = queueCapacity
            };

            MessageBuffer workerQueue = new MessageBuffer(queueOptions);
            ISubscriptionTag subscriberTag = subscriber.SubscribeTo(workerQueue);

            ISubscriptionTag dispatcherTag = this.LinkDispatcherTo<T>(workerQueue);

            subscriberTag = new CompositeSubscriptionTag(Guid.NewGuid().ToString(), subscriberTag, dispatcherTag);
            subscribers.AddOrUpdate(subscriberTag.Id, subscriberTag, (o, n) => n);

            return subscriberTag;
        }

        public Task<bool> EmitAsync<T>(T payload, IDictionary<string, string> headers)
        {
            if (this.inboxes.TryGetValue(typeof(T), out MessageBuffer inbox))
            {
                this.logService.Trace("Sending the message to the inbox of {0}.", typeof(T));
                return inbox.SendAsync(new Message<T>(payload, headers));
            }
            else
            {
                this.logService.Trace("Sending the message is failed due to absence any subscriptions to {0}.", typeof(T));
                return Task.FromResult(false);
            }
        }

        public int GetInboxMessageCount<T>()
        {
            if (this.inboxes.TryGetValue(typeof(T), out MessageBuffer inbox))
            {
                return inbox.Count;
            }
            else
            {
                return 0;
            }

        }

        public bool IsSubscribed(ISubscriptionTag tag)
        {
            return subscribers.ContainsKey(tag.Id);
        }

        public void Unsubscribe(ISubscriptionTag tag)
        {
            this.logService.Info("Unsubscribeing a subscription.");
            tag.Dispose();
            this.subscribers.TryRemove(tag.Id, out var subscriber);
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
            var dispatcher = this.dispatchers.GetOrAdd(typeof(T), new MessageDispatcher());

            var inboxQueue = this.CreateInboxWithDispatcher<T>(dispatcher);

            IPublishingTag tag = publisher.LinkTo(inboxQueue);
            tag = this.publishers.AddOrUpdate(tag.Id, tag, (id, t) => t);

            return tag;
        }

        private ISubscriptionTag LinkDispatcherTo<T>(ITargetBlock<IMessage> target)
        {
            var dispatcher = this.dispatchers.GetOrAdd(typeof(T), new MessageDispatcher(this.logServiceProvider));
            IDisposable dispatchDisposable = dispatcher.Subscribe<T>(target);

            this.CreateInboxWithDispatcher<T>(dispatcher);

            string subsriptionId = Guid.NewGuid().ToString();
            ISubscriptionTag dispatcherTag = new SubscriptionTag(subsriptionId, dispatchDisposable);

            return dispatcherTag;
        }

        private MessageBuffer CreateInboxWithDispatcher<T>(MessageDispatcher dispatcher)
        {
            this.logService.Info("Registering dispatcher of {0}.", typeof(T));
            var dispatch = new MessageAction(dispatcher.Dispatch, this.defaultDistpatcherOptions);
            var inboxQueue = new MessageBuffer(this.defaultInboxQueueOptions);

            inboxQueue.LinkTo(dispatch);
            inboxQueue = this.inboxes.GetOrAdd(typeof(T), inboxQueue);

            return inboxQueue;
        }


    }
}
