using System.Threading.Tasks.Dataflow;

namespace Kontur
{
    public interface IMessageAction
    {
        ITargetBlock<IMessage> AsTarget { get; }

        void Complete();
    }
}
