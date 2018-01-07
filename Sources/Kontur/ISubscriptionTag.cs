using System;

namespace Kontur
{
    public interface ISubscriptionTag : IDisposable
    {
        Guid Id { get; }
    }
}
