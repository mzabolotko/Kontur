using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public class MessageBuffer : IMessageBuffer
    {
        private DataflowBlockOptions defaultInboxQueueOptions;
        private readonly BufferBlock<IMessage> bufferBlock;

        public MessageBuffer(DataflowBlockOptions defaultInboxQueueOptions)
        {
            this.defaultInboxQueueOptions = defaultInboxQueueOptions;
            this.bufferBlock = new BufferBlock<Kontur.IMessage>(this.defaultInboxQueueOptions);
        }

        public void LinkTo(IMessageAction messageAction)
        {
            this.bufferBlock.LinkTo(messageAction.AsTarget);
        }

        public Task<bool> SendAsync(IMessage message, CancellationToken concellationToken)
        {
            return this.bufferBlock.SendAsync(message, concellationToken);
        }

        public int Count => this.bufferBlock.Count;

        public ITargetBlock<IMessage> AsTarget => this.bufferBlock;

        public ISourceBlock<IMessage> AsSource => this.bufferBlock;
    }
}
