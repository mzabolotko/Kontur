using System;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public class Exchange : IExchange
    {
        private readonly IMessageDispatcherFactory messageDispatcherFactory;
        private readonly ILogServiceProvider logServiceProvider;
        private readonly ILogService logService;
        private readonly ConcurrentDictionary<Type, IMessageDispatcher> dispatchers = new ConcurrentDictionary<Type, IMessageDispatcher>();

        public Exchange(IMessageDispatcherFactory messageDispatcherFactory, ILogServiceProvider logServiceProvider)
        {
            this.messageDispatcherFactory = messageDispatcherFactory;
            this.logServiceProvider = logServiceProvider;
            this.logService = this.logServiceProvider.GetLogServiceOf(typeof(Exchange));
            this.logService.Debug("Created an exchange.");
        }

        public ISubscriptionTag BindSubscriberQueue<T>(IInbox inbox, ITargetBlock<IMessage> queue)
        {
            this.logService.Debug("Binding a subscriber queue to the inbox.");
            IMessageDispatcher dispatcher = this.dispatchers.GetOrAdd(typeof(T), this.messageDispatcherFactory.Create());
            IDisposable dispatchDisposable = dispatcher.Subscribe<T>(queue);

            string subsriptionId = Guid.NewGuid().ToString();
            ISubscriptionTag dispatcherTag = new SubscriptionTag(subsriptionId, dispatchDisposable);

            inbox.CreateInboxWithDispatcher<T>(dispatcher.Dispatch);

            return dispatcherTag;
        }

        public IMessageBuffer BindPublisher<T>(IInbox inbox)
        {
            this.logService.Debug("Binding a publisher to the inbox.");
            IMessageDispatcher dispatcher =
                this.dispatchers.GetOrAdd(
                    typeof(T),
                    this.messageDispatcherFactory.Create());

            var inboxQueue = inbox.CreateInboxWithDispatcher<T>(dispatcher.Dispatch);

            return inboxQueue;
        }
    }
}
