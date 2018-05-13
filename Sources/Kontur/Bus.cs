using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public class Bus : IPublisherRegistry, ISubscriptionRegistry
    {
        private readonly BufferBlock<IMessage> inbox = new BufferBlock<IMessage>();
        private readonly BroadcastBlock<IMessage> dispatcher = new BroadcastBlock<IMessage>(message => message);
        private readonly ConcurrentDictionary<string, ISubscriptionTag> subscribers = new ConcurrentDictionary<string, ISubscriptionTag>();

        public Bus()
        {
            this.inbox.LinkTo(dispatcher);
        }

        public ISubscriptionTag Subscribe<T>(Action<Message<T>> subscriber)
        {
            var workerQueue = new BufferBlock<IMessage>();
            ISubscriptionTag workerQueueTag = this.LinkToDispatcher<T>(workerQueue);

            var worker = new ActionBlock<IMessage>(m => subscriber(this.As<T>(m)));
            workerQueue.LinkTo(worker);

            return workerQueueTag;
        }

        public ISubscriptionTag Subscribe<T>(ISubscriber subscriber)
        {
            var workerQueue = new BufferBlock<IMessage>();

            ISubscriptionTag workerQueueTag = this.LinkToDispatcher<T>(workerQueue);

            ISubscriptionTag tag = subscriber.LinkTo(workerQueue);
            return new CompositeSubscriptionTag(
                Guid.NewGuid().ToString(),
                new List<ISubscriptionTag>
                {
                    tag,
                    workerQueueTag
                });
        }

        public IPublishingTag RegisterPublisher(IPublisher publisher)
        {
            return publisher.LinkTo(this.inbox);
        }

        public async Task<bool> EmitAsync<T>(T payload, IDictionary<string, string> headers)
        {
            return await this.inbox.SendAsync(new Message<T>(payload, headers));
        }

        public int InboxMessageCount => this.inbox.Count;

        public bool IsSubscribed(ISubscriptionTag tag)
        {
            return subscribers.ContainsKey(tag.Id);
        }

        public void Unsubscribe(ISubscriptionTag tag)
        {
            tag.Dispose();
            this.subscribers.TryRemove(tag.Id, out var worker);
        }

        private Message<T> As<T>(IMessage message)
        {
            return (message as Message<T>);
        }

        private ISubscriptionTag LinkToDispatcher<T>(ITargetBlock<IMessage> target)
        {
            IDisposable link = this.dispatcher.LinkTo(target, message => typeof(T) == message.RouteKey);

            string subscriptionId = Guid.NewGuid().ToString();
            ISubscriptionTag dispatcherTag = new SubscriptionTag(subscriptionId, link);
            dispatcherTag = subscribers.AddOrUpdate(subscriptionId, dispatcherTag, (o, n) => n);

            return dispatcherTag;
        }
    }
}
