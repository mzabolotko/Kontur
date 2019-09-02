using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public class MessageBufferFactory : IMessageBufferFactory
    {
        private readonly DataflowBlockOptions defaultInboxQueueOptions;

        public MessageBufferFactory(int defaultCapacity = 10)
        {
            this.defaultInboxQueueOptions = new DataflowBlockOptions
            {
                BoundedCapacity = defaultCapacity
            };
        }

        public IMessageBuffer Create(int? capacity)
        {

            if (capacity != null)
            {
                DataflowBlockOptions queueOptions = new DataflowBlockOptions
                {
                    BoundedCapacity = capacity.Value
                };
                return new MessageBuffer(this.defaultInboxQueueOptions);
            }

            return new MessageBuffer(this.defaultInboxQueueOptions);
        }
    }
}
