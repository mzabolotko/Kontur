using System;

namespace Kontur
{
    public interface ILogServiceProvider
    {
        ILogService GetLogServiceOf(Type type);
    }
}
