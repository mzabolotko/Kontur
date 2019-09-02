using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public interface IExchange
    {
        IMessageBuffer BindPublisher<T>(IInbox inbox);
        ISubscriptionTag BindSubscriber<T>(IInbox inbox, ITargetBlock<IMessage> target);
    }
}
