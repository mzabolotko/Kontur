using System;

namespace Kontur
{
    public interface ILogService
    {
        void Trace(string format, params object[] args);
        void Debug(string format, params object[] args);
        void Info(string format, params object[] args);
        void Warn(string format, params object[] args);
        void Error(string format, params object[] args);
        void Fatal(string format, params object[] args);

        void Warn(Exception ex, string format, params object[] args);
        void Error(Exception ex, string format, params object[] args);
        void Fatal(Exception ex, string format, params object[] args);
    }
}
