namespace Kontur
{
    public class MessageDispatcherFactory : IMessageDispatcherFactory
    {
        private readonly ILogServiceProvider logServiceProvider;

        public MessageDispatcherFactory(ILogServiceProvider logServiceProvider)
        {
            this.logServiceProvider = logServiceProvider;
        }

        public IMessageDispatcher Create()
        {
            return new MessageDispatcher(this.logServiceProvider);
        }
    }
}
