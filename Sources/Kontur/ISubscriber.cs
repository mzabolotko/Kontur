using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public interface ISubscriber
    {
        ISubscriptionTag SubscribeTo(ISourceBlock<IMessage> target);
    }

}
