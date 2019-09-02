namespace Kontur
{
    public interface IMessageBufferFactory
    {
        IMessageBuffer Create(int? capacity = null);
    }
}
