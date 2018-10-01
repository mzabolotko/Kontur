using System;

namespace Kontur
{
    public class NullLogServiceProvider : ILogServiceProvider
    {
        public ILogService GetLogServiceOf(Type type)
        {
            return new NullLogService();
        }
    }
}
