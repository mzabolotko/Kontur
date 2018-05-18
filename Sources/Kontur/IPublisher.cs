using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public interface IPublisher
    {
        IPublishingTag LinkTo(ITargetBlock<IMessage> target);
    }
}
