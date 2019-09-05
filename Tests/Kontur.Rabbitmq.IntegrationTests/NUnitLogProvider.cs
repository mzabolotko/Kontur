using System;

namespace Kontur.Rabbitmq.IntegrationTests
{
    class NUnitLogProvider : ILogServiceProvider
    {
        public ILogService GetLogServiceOf(Type type)
        {
            return new NUnitLogService(type);
        }
    }
}
