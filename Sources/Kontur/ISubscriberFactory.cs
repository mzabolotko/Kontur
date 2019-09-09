using System;

namespace Kontur
{
    public interface ISubscriberFactory
    {
        ISubscriber Create<T>(Action<Message<T>> action);
    }
}
