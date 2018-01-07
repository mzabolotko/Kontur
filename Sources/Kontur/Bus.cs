using System;
using System.Collections.Concurrent;
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

        public ISubscriptionTag Subscribe(string routeKey, Action<IMessage> subscriber)
        {
            BufferBlock<IMessage> workerQueue = new BufferBlock<IMessage>();
            ActionBlock<IMessage> worker = new ActionBlock<IMessage>(subscriber);

            workerQueue.LinkTo(worker);

            IDisposable link = this.dispatcher.LinkTo(workerQueue, message => routeKey == message.RouteKey);
            Guid subsriptionId = Guid.NewGuid();
            link = subscribers.AddOrUpdate(subsriptionId, link, (o, n) => n);

            return new SubscriptionTag(subsriptionId, link);
        }

        public async Task<bool> EmitAsync<T>(string routeKey, T payload)
        {
            return await this.inbox.SendAsync(new Message<T>(routeKey, payload));
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
