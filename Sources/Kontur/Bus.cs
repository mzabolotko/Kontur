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
        private readonly ConcurrentDictionary<Type, MessageBuffer> inboxes;
        private readonly ConcurrentDictionary<Type, MessageDispatcher> dispatchers;
        private readonly ConcurrentDictionary<string, ISubscriptionTag> subscribers;
        private readonly ConcurrentDictionary<string, IPublishingTag> publishers;

        private readonly ExecutionDataflowBlockOptions defaultDistpatcherOptions;
        private readonly DataflowBlockOptions defaultInboxQueueOptions;

        public Bus(int inboxCapacity = 10)
        {
            this.inboxes = new ConcurrentDictionary<Type, MessageBuffer>();
            this.dispatchers = new ConcurrentDictionary<Type, MessageDispatcher>();
            this.subscribers = new ConcurrentDictionary<string, ISubscriptionTag>();
            this.publishers = new ConcurrentDictionary<string, IPublishingTag>();

            this.defaultInboxQueueOptions = new DataflowBlockOptions
                {
                    BoundedCapacity = inboxCapacity
                };

            this.defaultDistpatcherOptions = new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = 1
                };
        }

        public ISubscriptionTag Subscribe<T>(Action<Message<T>> subscriber, int queueCapacity = 1)
        {
            MessageSubscriber<T> messageSubscriber = new MessageSubscriber<T>(subscriber);
            return this.Subscribe<T>(messageSubscriber, queueCapacity);
        }

        public ISubscriptionTag Subscribe<T>(ISubscriber subscriber, int queueCapacity = 1)
        {
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
                return inbox.SendAsync(new Message<T>(payload, headers));
            }
            else
            {
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
            tag.Dispose();
            this.subscribers.TryRemove(tag.Id, out var subscriber);
        }

        public bool IsRegistered(IPublishingTag tag)
        {
            return publishers.ContainsKey(tag.Id);
        }

        public void Unregister(IPublishingTag tag)
        {
            tag.Dispose();
            this.publishers.TryRemove(tag.Id, out var publisher);
        }

        public IPublishingTag RegisterPublisher<T>(IPublisher publisher)
        {
            var dispatcher = this.dispatchers.GetOrAdd(typeof(T), new MessageDispatcher());

            var inboxQueue = this.CreateInboxWithDispatcher<T>(dispatcher);

            IPublishingTag tag = publisher.LinkTo(inboxQueue);
            tag = this.publishers.AddOrUpdate(tag.Id, tag, (id, t) => t);

            return tag;
        }

        private ISubscriptionTag LinkDispatcherTo<T>(ITargetBlock<IMessage> target)
        {
            var dispatcher = this.dispatchers.GetOrAdd(typeof(T), new MessageDispatcher());
            IDisposable dispatchDisposable = dispatcher.Subscribe<T>(target);

            this.CreateInboxWithDispatcher<T>(dispatcher);

            string subsriptionId = Guid.NewGuid().ToString();
            ISubscriptionTag dispatcherTag = new SubscriptionTag(subsriptionId, dispatchDisposable);

            return dispatcherTag;
        }

        private MessageBuffer CreateInboxWithDispatcher<T>(MessageDispatcher dispatcher)
        {
            var dispatch = new MessageAction(dispatcher.Dispatch, this.defaultDistpatcherOptions);
            var inboxQueue = new MessageBuffer(this.defaultInboxQueueOptions);

            inboxQueue.LinkTo(dispatch);
            inboxQueue = this.inboxes.GetOrAdd(typeof(T), inboxQueue);

            return inboxQueue;
        }


    }
}
