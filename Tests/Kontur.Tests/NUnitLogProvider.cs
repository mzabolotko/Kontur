using System;

namespace Kontur.Tests
{
    class NUnitLogProvider : ILogServiceProvider
    {
        public ILogService GetLogServiceOf(Type type)
        {
            return new NUnitLogService(type);
        }
    }
}
