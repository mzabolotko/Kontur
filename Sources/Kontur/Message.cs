using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kontur
{
    public class Message<T> : IMessage
    {
        public Message(T payload, IDictionary<string, string> headers, TaskCompletionSource<bool> tcs)
            : this(payload, headers)
        {
            this.TaskCompletionSource = tcs;
        }

        public Message(T payload, IDictionary<string, string> headers)
        {
            this.RouteKey = typeof(T);
            this.Payload = payload;
            this.Headers = (IReadOnlyDictionary<string, string>) headers;
        }

        public Type RouteKey { get; }

        public IReadOnlyDictionary<string, string> Headers { get; }

        object IMessage.Payload => this.Payload;

        public T Payload { get; }

        public TaskCompletionSource<bool> TaskCompletionSource { get; }
    }
}
