using System;

namespace Kontur
{
    internal class MessageDispatcherTag : IDisposable
    {
        private readonly string id;
        private readonly Action<string> unsubcribe;

        public MessageDispatcherTag(string id, Action<string> unsubcribe)
        {
            this.id = id;
            this.unsubcribe = unsubcribe;
        }

        public void Dispose()
        {
            unsubcribe(this.id);
        }
    }
}
