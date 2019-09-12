using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public interface IMessageDispatcher
    {
        Task Dispatch(IMessage message);
        IDisposable Subscribe<T>(ITargetBlock<IMessage> target);
    }
}
