using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using MessageAction = System.Threading.Tasks.Dataflow.ActionBlock<Kontur.IMessage>;

namespace Kontur
{
    internal class MessageSubscriber<T> : ISubscriber, IDisposable
    {
        private readonly ExecutionDataflowBlockOptions defaultConsumerOptions =
            new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 1
            };

        private readonly MessageAction worker;

        private readonly Action<Message<T>> action;
        private readonly ILogServiceProvider logServiceProvider;
        private readonly ILogService logService;
        private bool disposed = false;

        public MessageSubscriber(Action<Message<T>> action, ILogServiceProvider logServiceProvider)
        {
            this.action = action;
            this.logServiceProvider = logServiceProvider;
            this.logService = this.logServiceProvider.GetLogServiceOf(typeof(MessageSubscriber<T>));
            this.worker = new MessageAction((Action<IMessage>)this.OnMessage, this.defaultConsumerOptions);
        }

        public ISubscriptionTag SubscribeTo(ISourceBlock<IMessage> target)
        {
            target.LinkTo(this.worker);
            return new SubscriptionTag(Guid.NewGuid().ToString(), this);
        }

        private void OnMessage(IMessage message)
        {
            try
            {
                this.logService.Debug("Processing the message.");
                action(this.As(message));
            }
            catch (System.Exception ex)
            {
                this.logService.Error("Processing the message was failed.", ex);
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
