using System;

namespace Kontur.Rabbitmq
{
    internal class SubscribingTag : ISubscriptionTag
    {
        private readonly string id;
        private readonly Action cancelSending;

        public SubscribingTag(string id, Action cancelSending)
        {
            this.id = id;
            this.cancelSending = cancelSending;
        }

        public string Id => this.id;

        public void Dispose()
        {
            cancelSending();
        }
    }

}
