using System;

namespace Kontur.Rabbitmq.Tests
{
    internal class LogServiceProvider : ILogServiceProvider
    {
        public ILogService GetLogServiceOf(Type type)
        {
            return new ConsoleLogService(type);
        }
    }
}
