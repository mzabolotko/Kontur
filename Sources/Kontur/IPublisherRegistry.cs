namespace Kontur
{
    public interface IPublisherRegistry
    {
        IPublishingTag RegisterPublisher<T>(IPublisher publisher);
        bool IsRegistered(IPublishingTag tag);
        void Unregister(IPublishingTag tag);
    }
}