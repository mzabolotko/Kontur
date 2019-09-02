using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public class MessageActionFactory : IMessageActionFactory
    {
        private readonly ExecutionDataflowBlockOptions defaultDistpatcherOptions;

        public MessageActionFactory()
        {
            this.defaultDistpatcherOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 1
            };
        }

        public IMessageAction Create(Action<IMessage> dispatch)
        {
            return new MessageAction(dispatch, this.defaultDistpatcherOptions);
        }

        public IMessageAction Create(Func<IMessage, Task> dispatch)
        {
            return new MessageAction(dispatch, this.defaultDistpatcherOptions);
        }
    }
}
