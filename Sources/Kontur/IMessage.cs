using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kontur
{
    public interface IMessage
    {
        Type RouteKey { get; }

        IReadOnlyDictionary<string, string> Headers { get; }

        object Payload { get; }

        TaskCompletionSource<bool> TaskCompletionSource { get; }
    }
}
