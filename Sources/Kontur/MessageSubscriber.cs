using System;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    internal class MessageSubscriber<T> : ISubscriber, IDisposable
    {
        private readonly ILogServiceProvider logServiceProvider;
        private readonly IMessageAction worker;
        private readonly Action<Message<T>> action;
        private readonly ILogService logService;
        private bool disposed = false;

        public MessageSubscriber(Action<Message<T>> action, IMessageActionFactory messageActionFactory, ILogServiceProvider logServiceProvider)
        {
            this.action = action;
            this.worker = messageActionFactory.Create((Action<IMessage>)this.OnMessage);
            this.logServiceProvider = logServiceProvider ?? new NullLogServiceProvider();
            this.logService = this.logServiceProvider.GetLogServiceOf(typeof(MessageSubscriber<T>));
            this.logService.Debug("Created the message subscriber.");
        }

        public ISubscriptionTag SubscribeTo(ISourceBlock<IMessage> target)
        {
            target.LinkTo(this.worker.AsTarget);
            return new SubscriptionTag(Guid.NewGuid().ToString(), this);
        }

        private void OnMessage(IMessage message)
        {
            Message<T> m = this.As(message);
            try
            {
                this.logService.Debug("Processing the message.");
                action(m);
                m.TaskCompletionSource.TrySetResult(true);
            }
            catch (System.Exception ex)
            {
                this.logService.Error("Processing the message was failed.", ex);
                m.TaskCompletionSource.TrySetResult(true);
            }
        }

        private Message<T> As(IMessage message) => (message as Message<T>);

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.worker.Complete();

                this.disposed = true;
            }
        }
    }
}
