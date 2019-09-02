using System;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public class Exchange : IExchange
    {
        private readonly ILogServiceProvider logServiceProvider;
        private readonly ILogService logService;
        private readonly ConcurrentDictionary<Type, MessageDispatcher> dispatchers;

        public Exchange(ILogServiceProvider logServiceProvider = null)
        {
            this.logServiceProvider = logServiceProvider ?? new NullLogServiceProvider();
            this.logService = this.logServiceProvider.GetLogServiceOf(typeof(Exchange));
            this.dispatchers = new ConcurrentDictionary<Type, MessageDispatcher>();
            this.logService.Debug("Created an exchange.");
        }

        public ISubscriptionTag BindSubscriber<T>(IInbox inbox, ITargetBlock<IMessage> target)
        {
            this.logService.Debug("Binding a subscriber to the inbox.");
            var dispatcher = this.dispatchers.GetOrAdd(typeof(T), new MessageDispatcher(this.logServiceProvider));
            IDisposable dispatchDisposable = dispatcher.Subscribe<T>(target);

            string subsriptionId = Guid.NewGuid().ToString();
            ISubscriptionTag dispatcherTag = new SubscriptionTag(subsriptionId, dispatchDisposable);

            inbox.CreateInboxWithDispatcher<T>(dispatcher);

            return dispatcherTag;
        }

        public IMessageBuffer BindPublisher<T>(IInbox inbox)
        {
            this.logService.Debug("Binding a publisher to the inbox.");
            var dispatcher = this.dispatchers.GetOrAdd(typeof(T), new MessageDispatcher());

            var inboxQueue = inbox.CreateInboxWithDispatcher<T>(dispatcher);

            return inboxQueue;
        }
    }
}
