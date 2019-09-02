using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public interface IMessageBuffer
    {
        int Count { get; }
        void LinkTo(IMessageAction messageAction);
        Task<bool> SendAsync(IMessage message, CancellationToken conselationToken);
        ITargetBlock<IMessage> AsTarget { get; }
        ISourceBlock<IMessage> AsSource { get; }
    }
}
