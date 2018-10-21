using System;

namespace Kontur.Tests
{
    internal class LogServiceProvider : ILogServiceProvider
    {
        public ILogService GetLogServiceOf(Type type)
        {
            return new ConsoleLogService(type);
        }
    }
}
