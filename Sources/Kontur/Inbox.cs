using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Kontur
{
    public class Inbox : IInbox
    {
        private readonly IMessageBufferFactory messageBufferFactory;
        private readonly IMessageActionFactory messageActionFactory;
        private readonly ConcurrentDictionary<Type, IMessageBuffer> inboxes;
        private readonly ILogServiceProvider logServiceProvider;
        private readonly ILogService logService;

        public Inbox(IMessageBufferFactory messageBufferFactory, IMessageActionFactory messageActionFactory, ILogServiceProvider logServiceProvider = null)
        {
            this.messageBufferFactory = messageBufferFactory;
            this.messageActionFactory = messageActionFactory;
            this.inboxes = new ConcurrentDictionary<Type, IMessageBuffer>();
            this.logServiceProvider = logServiceProvider ?? new NullLogServiceProvider();
            this.logService = this.logServiceProvider.GetLogServiceOf(typeof(Inbox));
            this.logService.Info("Created an inbox.");
        }

        public Task EmitAsync<T>(IMessage message)
        {
            var cancelTokenSource = new System.Threading.CancellationTokenSource();
            if (this.inboxes.TryGetValue(typeof(T), out IMessageBuffer inbox))
            {
                this.logService.Trace("Sending the message to the inbox of {0}.", typeof(T));
                var t = inbox.SendAsync(message, cancelTokenSource.Token);
                t.Wait();

                return message.TaskCompletionSource.Task;
            }
            else
            {
                this.logService.Trace("Sending the message is failed due to absence any subscriptions to {0}.", typeof(T));
                return Task.FromResult(false);
            }
        }

        public int GetInboxMessageCount<T>()
        {
            if (this.inboxes.TryGetValue(typeof(T), out IMessageBuffer inbox))
            {
                return inbox.Count;
            }
            else
            {
                return 0;
            }
        }

        public IMessageBuffer CreateInboxWithDispatcher<T>(MessageDispatcher dispatcher)
        {
            this.logService.Info("Registering dispatcher of {0}.", typeof(T));
            var dispatch = this.messageActionFactory.Create(dispatcher.Dispatch);
            var inboxQueue = this.messageBufferFactory.Create();

            inboxQueue.LinkTo(dispatch);
            inboxQueue = this.inboxes.GetOrAdd(typeof(T), inboxQueue);

            return inboxQueue;
        }
    }
}
