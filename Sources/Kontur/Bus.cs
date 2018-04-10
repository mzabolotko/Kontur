using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public class Bus
    {
        private readonly BufferBlock<IMessage> inbox = new BufferBlock<IMessage>();
        private readonly BroadcastBlock<IMessage> dispatcher = new BroadcastBlock<IMessage>(message => message);
        private readonly ConcurrentDictionary<Guid, IDisposable> subscribers = new ConcurrentDictionary<Guid, IDisposable>();

        public Bus()
        {
            this.inbox.LinkTo(dispatcher);
        }

        public ISubscriptionTag Subscribe<T>(Action<IMessage> subscriber)
        {
            ActionBlock<IMessage> worker = new ActionBlock<IMessage>(subscriber);

            return Subscribe<T>(worker);
        }

        public ISubscriptionTag Subscribe<T>(ITargetBlock<IMessage> subscriber)
        {
            BufferBlock<IMessage> workerQueue = new BufferBlock<IMessage>();
            workerQueue.LinkTo(subscriber);

            IDisposable link = this.dispatcher.LinkTo(workerQueue, message => typeof(T) == message.RouteKey);
            Guid subsriptionId = Guid.NewGuid();
            link = subscribers.AddOrUpdate(subsriptionId, link, (o, n) => n);

            return new SubscriptionTag(subsriptionId, link);
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
    }
}
