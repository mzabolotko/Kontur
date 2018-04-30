using System;

namespace Kontur
{
    internal class SubscriptionTag : ISubscriptionTag
    {
        private readonly string id;
        private readonly IDisposable disposable;

        public SubscriptionTag(string id, IDisposable disposable)
        {
            this.id = id;
            this.disposable = disposable;
        }

        public string Id => this.id;

        public void Dispose()
        {
            this.disposable.Dispose();
        }
    }
}
