using System;

namespace Kontur
{
    internal class SubscriberFactory : ISubscriberFactory
    {
        private readonly IMessageActionFactory messageActionFactory;
        private readonly ILogServiceProvider logServiceProvider;

        public SubscriberFactory(IMessageActionFactory messageActionFactory, ILogServiceProvider logServiceProvider)
        {
            this.messageActionFactory = messageActionFactory;
            this.logServiceProvider = logServiceProvider;
        }

        public ISubscriber Create<T>(Action<Message<T>> action)
        {
            return new MessageSubscriber<T>(action, this.messageActionFactory, this.logServiceProvider);
        }
    }
}
