using System;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public class Exchange : IExchange
    {
        private readonly ILogServiceProvider logServiceProvider;
        private readonly ILogService logService;
        private readonly ConcurrentDictionary<Type, MessageDispatcher> dispatchers = new ConcurrentDictionary<Type, MessageDispatcher>();

        public Exchange(ILogServiceProvider logServiceProvider)
        {
            this.logServiceProvider = logServiceProvider;
            this.logService = this.logServiceProvider.GetLogServiceOf(typeof(Exchange));
            this.logService.Debug("Created an exchange.");
        }

        public ISubscriptionTag BindSubscriberQueue<T>(IInbox inbox, ITargetBlock<IMessage> queue)
        {
            this.logService.Debug("Binding a subscriber queue to the inbox.");
            var dispatcher = this.dispatchers.GetOrAdd(typeof(T), new MessageDispatcher(this.logServiceProvider));
            IDisposable dispatchDisposable = dispatcher.Subscribe<T>(queue);

            string subsriptionId = Guid.NewGuid().ToString();
            ISubscriptionTag dispatcherTag = new SubscriptionTag(subsriptionId, dispatchDisposable);

            inbox.CreateInboxWithDispatcher<T>(dispatcher.Dispatch);

            return dispatcherTag;
        }

        public IMessageBuffer BindPublisher<T>(IInbox inbox)
        {
            this.logService.Debug("Binding a publisher to the inbox.");
            var dispatcher = this.dispatchers.GetOrAdd(typeof(T), new MessageDispatcher(this.logServiceProvider));

            var inboxQueue = inbox.CreateInboxWithDispatcher<T>(dispatcher.Dispatch);

            return inboxQueue;
        }
    }
}
