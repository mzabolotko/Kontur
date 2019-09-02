using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public class MessageAction : IMessageAction
    {
        private ExecutionDataflowBlockOptions defaultDistpatcherOptions;
        private ActionBlock<IMessage> actionBlock;

        public MessageAction(Action<IMessage> dispatch, ExecutionDataflowBlockOptions defaultDistpatcherOptions)
        {
            this.defaultDistpatcherOptions = defaultDistpatcherOptions;
            this.actionBlock = new ActionBlock<Kontur.IMessage>(dispatch, this.defaultDistpatcherOptions);
        }

        public MessageAction(Func<IMessage, Task> dispatch, ExecutionDataflowBlockOptions defaultDistpatcherOptions)
        {
            this.defaultDistpatcherOptions = defaultDistpatcherOptions;
            this.actionBlock = new ActionBlock<Kontur.IMessage>(dispatch, this.defaultDistpatcherOptions);
        }

        public ITargetBlock<IMessage> AsTarget => this.actionBlock;

        public void Complete()
        {
            this.actionBlock.Complete();
        }
    }
}
