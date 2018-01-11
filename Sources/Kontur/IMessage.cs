using System.Collections.Generic;

namespace Kontur
{
    public interface IMessage
    {
        string RouteKey { get; }

        IReadOnlyDictionary<string, string> Headers { get; }
    }
}
