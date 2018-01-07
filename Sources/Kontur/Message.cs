namespace Kontur
{
    internal class Message<T> : IMessage
    {
        private readonly string routeKey;
        private readonly T payload;

        public Message(string routeKey, T payload)
        {
            this.routeKey = routeKey;
            this.payload = payload;
        }

        public string RouteKey => routeKey;
    }
}
