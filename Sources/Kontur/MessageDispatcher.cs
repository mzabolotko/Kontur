using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    internal class MessageDispatcher
    {
        ConcurrentDictionary<Type, ConcurrentDictionary<string, ITargetBlock<IMessage>>> routes =
            new ConcurrentDictionary<Type, ConcurrentDictionary<string, ITargetBlock<IMessage>>>();

        internal IDisposable Subscribe<T>(ITargetBlock<IMessage> target)
        {
            var id = Guid.NewGuid().ToString();
            var items = new List<KeyValuePair<string, ITargetBlock<IMessage>>>
            {
                new KeyValuePair<string, ITargetBlock<IMessage>>(id, target)
            };
            var addValue = new ConcurrentDictionary<string, ITargetBlock<IMessage>>(items);

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
            if (routes.TryGetValue(message.RouteKey, out ConcurrentDictionary<string, ITargetBlock<IMessage>> subscribers))
            {
                var tasks = subscribers.Values.Select(target => target.SendAsync(message));
                return Task.WhenAll(tasks);
            }

            return Task.CompletedTask;
        }

        private bool Unsubscribe<T>(string id)
        {
            if (routes.TryGetValue(typeof(T), out ConcurrentDictionary<string, ITargetBlock<IMessage>> value))
            {
                if (value.TryRemove(id, out ITargetBlock<IMessage> target))
                {
                    target.Complete();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal int GetCountSubscriberOf(Type type)
        {
            if (routes.TryGetValue(type, out ConcurrentDictionary<string, ITargetBlock<IMessage>> value))
            {
                return value.Count;
            }
            else
            {
                return 0;
            }
        }

    }
}
