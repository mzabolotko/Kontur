using System;
using System.Threading.Tasks;

namespace Kontur
{
    public interface IInbox
    {
        Task<bool> EmitAsync<T>(IMessage message);

        int GetInboxMessageCount<T>();
        IMessageBuffer CreateInboxWithDispatcher<T>(Func<IMessage, Task> dispatch);
        IPublishingTag RegisterPublisher<T>(IPublisher publisher, IMessageBuffer inboxQueue);
        bool IsRegistered(IPublishingTag tag);
        void Unregister(IPublishingTag tag);
    }
}
