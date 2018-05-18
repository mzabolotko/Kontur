using System.Collections.Generic;

namespace Kontur
{
    public class CompositePublishingTag : IPublishingTag
    {
        private readonly string id;
        private readonly List<IPublishingTag> publishingTags;

        public CompositePublishingTag(string id, List<IPublishingTag> publishingTags)
        {
            this.id = id;
            this.publishingTags = publishingTags;
        }

        public string Id => this.id;

        public void Dispose()
        {
            foreach (var tag in this.publishingTags)
            {
                tag.Dispose();
            }
        }
    }
}
