using System.Collections.Generic;

namespace Kontur
{
    public class CompositeSubscriptionTag : ISubscriptionTag
    {
        private readonly List<ISubscriptionTag> subscriptionTags;
        private string id;

        public CompositeSubscriptionTag(string id, List<ISubscriptionTag> subscriptionTags)
        {
            this.id = id;
            this.subscriptionTags = subscriptionTags;
        }

        public CompositeSubscriptionTag(string id, params ISubscriptionTag[] tags)
        {
            this.id = id;
            this.subscriptionTags = new List<ISubscriptionTag>(tags);
        }

        public string Id => this.id;

        public void Dispose()
        {
            foreach (var tag in this.subscriptionTags)
            {
                tag.Dispose();
            }
        }
    }
}
