using System;

namespace Kontur
{
    public interface IPublishingTag : IDisposable
    {
        string Id { get; }
    }
}
