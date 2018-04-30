using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public interface ISubscriber
    {
        ISubscriptionTag LinkTo(ISourceBlock<IMessage> target);
    }

}
