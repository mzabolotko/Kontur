namespace Kontur
{
    public interface IPublisherRegistry
    {
        //TODO: unregister and check
        IPublishingTag RegisterPublisher(IPublisher publisher);
    }
}