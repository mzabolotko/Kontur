using System.Threading.Tasks;

namespace Kontur
{
    public interface IInbox
    {
        Task EmitAsync<T>(IMessage message);
        int GetInboxMessageCount<T>();
        IMessageBuffer CreateInboxWithDispatcher<T>(MessageDispatcher dispatcher);
        IPublishingTag RegisterPublisher<T>(IPublisher publisher, IMessageBuffer inboxQueue);
        bool IsRegistered(IPublishingTag tag);
        void Unregister(IPublishingTag tag);
    }
}
