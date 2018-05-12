using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public class Bus : IPublisherRegistry, ISubscriptionRegistry
    {
        private readonly ConcurrentDictionary<Type, BufferBlock<IMessage>> inboxes = new ConcurrentDictionary<Type, BufferBlock<IMessage>>();
        private readonly ConcurrentDictionary<Type, MessageDispatcher> dispatchers = new ConcurrentDictionary<Type, MessageDispatcher>();
        private readonly ConcurrentDictionary<string, ISubscriptionTag> subscribers = new ConcurrentDictionary<string, ISubscriptionTag>();
        private readonly ConcurrentDictionary<string, IPublishingTag> publishers = new ConcurrentDictionary<string, IPublishingTag>();

        public Bus()
        {
        }

        public ISubscriptionTag Subscribe<T>(Action<Message<T>> subscriber)
        {
            BufferBlock<IMessage> workerQueue = new BufferBlock<IMessage>(
                new DataflowBlockOptions
                {
                    BoundedCapacity = 1
                });
            ISubscriptionTag dispatcherTag = this.LinkToDispatcher<T>(workerQueue);

            var consumerOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 1
            };
            ActionBlock<IMessage> worker = new ActionBlock<IMessage>(
                m => subscriber(this.As<T>(m)),
                consumerOptions);
            workerQueue.LinkTo(worker);

            subscribers.AddOrUpdate(dispatcherTag.Id, dispatcherTag, (o, n) => n);

            return dispatcherTag;
        }

        public ISubscriptionTag Subscribe<T>(ISubscriber subscriber)
        {
            BufferBlock<IMessage> workerQueue = new BufferBlock<IMessage>();

            ISubscriptionTag dispatcherTag = this.LinkToDispatcher<T>(workerQueue);

            ISubscriptionTag tag = subscriber.LinkTo(workerQueue);
            tag = new CompositeSubscriptionTag(
                Guid.NewGuid().ToString(),
                new List<ISubscriptionTag>
                {
                    tag,
                    dispatcherTag
                });
            subscribers.AddOrUpdate(tag.Id, tag, (o, n) => n);

            return tag;
        }

        public IPublishingTag RegisterPublisher<T>(IPublisher publisher)
        {
            var dispatcher = new MessageDispatcher();
            dispatcher = this.dispatchers.GetOrAdd(typeof(T), dispatcher);

            var dispatch = new ActionBlock<IMessage>(
                dispatcher.Dispatch,
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = 1
                });
            var inbox = new BufferBlock<IMessage>(
                new DataflowBlockOptions
                {
                    BoundedCapacity = 10
                });
            inbox.LinkTo(dispatch);
            inbox = this.inboxes.GetOrAdd(typeof(T), inbox);

            IPublishingTag tag = publisher.LinkTo(inbox);
            tag = this.publishers.AddOrUpdate(tag.Id, tag, (id, t) => t);

            return tag;
        }

        public Task<bool> EmitAsync<T>(T payload, IDictionary<string, string> headers)
        {
            if (this.inboxes.TryGetValue(typeof(T), out BufferBlock<IMessage> inbox))
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
            if (this.inboxes.TryGetValue(typeof(T), out BufferBlock<IMessage> inbox))
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
            this.subscribers.TryRemove(tag.Id, out var worker);
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

        private ISubscriptionTag LinkToDispatcher<T>(ITargetBlock<IMessage> target)
        {
            var dispatcher = new MessageDispatcher();
            dispatcher = this.dispatchers.GetOrAdd(typeof(T), dispatcher);
            IDisposable dispatchTag = dispatcher.Subscribe<T>(target);

            var dispatch = new ActionBlock<IMessage>(
                dispatcher.Dispatch,
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = 1
                });
            var inbox = new BufferBlock<IMessage>(
                new DataflowBlockOptions
                {
                    BoundedCapacity = 10
                });
            inbox.LinkTo(dispatch);
            inbox = this.inboxes.GetOrAdd(typeof(T), inbox);

            string subsriptionId = Guid.NewGuid().ToString();
            ISubscriptionTag dispatcherTag = new SubscriptionTag(subsriptionId, dispatchTag);

            return dispatcherTag;
        }

        private Message<T> As<T>(IMessage message)
        {
            return (message as Message<T>);
        }
    }
}
