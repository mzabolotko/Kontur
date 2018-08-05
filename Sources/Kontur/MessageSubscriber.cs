using System;
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
        private bool disposed = false;

        public MessageSubscriber(Action<Message<T>> action)
        {
            this.worker = new MessageAction(m => action(this.As(m)), this.defaultConsumerOptions);
        }

        public ISubscriptionTag SubscribeTo(ISourceBlock<IMessage> target)
        {
            target.LinkTo(this.worker);
            return new SubscriptionTag(Guid.NewGuid().ToString(), this);
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
