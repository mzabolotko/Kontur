using System;

namespace Kontur.Rabbitmq
{
    class PublishingTag : IPublishingTag, IDisposable
    {
        private readonly string consumerTag;
        private readonly Action<string> cancelConsuming;

        public PublishingTag(string consumerTag, Action<string> cancelConsuming)
        {
            this.consumerTag = consumerTag;
            this.cancelConsuming = cancelConsuming;
        }

        public string Id => this.consumerTag;

        public void Dispose()
        {
            this.cancelConsuming(this.consumerTag);
        }
    }
}
