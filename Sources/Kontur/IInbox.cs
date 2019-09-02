using System.Threading.Tasks;

namespace Kontur
{
    public interface IInbox
    {
        Task EmitAsync<T>(IMessage message);
        int GetInboxMessageCount<T>();
        IMessageBuffer CreateInboxWithDispatcher<T>(MessageDispatcher dispatcher);
    }
}
