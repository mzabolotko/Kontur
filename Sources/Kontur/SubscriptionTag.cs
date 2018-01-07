using System;

namespace Kontur
{
    internal class SubscriptionTag : ISubscriptionTag
    {
        private readonly Guid id;
        private readonly IDisposable disposable;

        public SubscriptionTag(Guid id, IDisposable disposable)
        {
            this.id = id;
            this.disposable = disposable;
        }

        public Guid Id => this.id;

        public void Dispose()
        {
            this.disposable.Dispose();
        }
    }
}
