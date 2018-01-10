using System.Collections.Generic;

namespace Kontur
{
    public interface IMessage
    {
        string RouteKey { get; }

        IDictionary<string, string> Headers { get; }
    }
}
