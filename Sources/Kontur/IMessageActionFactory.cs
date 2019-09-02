using System;
using System.Threading.Tasks;

namespace Kontur
{
    public interface IMessageActionFactory
    {
        IMessageAction Create(Action<IMessage> dispatch);
        IMessageAction Create(Func<IMessage, Task> dispatch);
    }
}
