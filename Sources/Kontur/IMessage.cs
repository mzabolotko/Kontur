using System;
using System.Collections.Generic;

namespace Kontur
{
    public interface IMessage
    {
        Type RouteKey { get; }

        IReadOnlyDictionary<string, string> Headers { get; }

        object Payload { get; }
    }
}
