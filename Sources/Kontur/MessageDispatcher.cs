using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using MessageTarget = System.Threading.Tasks.Dataflow.ITargetBlock<Kontur.IMessage>;
using MessageTargetDictionary = System.Collections.Concurrent.ConcurrentDictionary<string, System.Threading.Tasks.Dataflow.ITargetBlock<Kontur.IMessage>>;

namespace Kontur
{
    internal class MessageDispatcher
    {
        private readonly ConcurrentDictionary<Type, MessageTargetDictionary> routes;

        public MessageDispatcher()
        {
            this.routes = new ConcurrentDictionary<Type, MessageTargetDictionary>();
        }

        internal IDisposable Subscribe<T>(ITargetBlock<IMessage> target)
        {
            var id = Guid.NewGuid().ToString();
            var items = new List<KeyValuePair<string, MessageTarget>>
            {
                new KeyValuePair<string, MessageTarget>(id, target)
            };
            var addValue = new MessageTargetDictionary(items);

            routes.AddOrUpdate(
                typeof(T),
                addValue,
                (t, d) =>
                {
                    d.AddOrUpdate(id, target, (i, nt) => nt);
                    return d;
                });
            return new MessageDispatcherTag(id, i => this.Unsubscribe<T>(i));
        }

        public Task Dispatch(IMessage message)
        {
            if (routes.TryGetValue(message.RouteKey, out MessageTargetDictionary subscribers))
            {
                var tasks = subscribers.Values.Select(target => target.SendAsync(message));
                return Task.WhenAll(tasks);
            }

            return Task.CompletedTask;
        }

        private bool Unsubscribe<T>(string id)
        {
            if (routes.TryGetValue(typeof(T), out MessageTargetDictionary value))
            {
                if (value.TryRemove(id, out MessageTarget target))
                {
                    target.Complete();
                    return true;
                }
            }

            return false;
        }

        internal int GetCountSubscriberOf(Type type)
        {
            if (routes.TryGetValue(type, out MessageTargetDictionary value))
            {
                return value.Count;
            }

            return 0;
        }

    }
}
