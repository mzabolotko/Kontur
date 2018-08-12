using System;

namespace Kontur
{
    internal class NullLogService : ILogService
    {
        public void Debug(string format, params object[] args)
        {
        }

        public void Error(string format, params object[] args)
        {
        }

        public void Error(Exception ex, string format, params object[] args)
        {
        }

        public void Fatal(string format, params object[] args)
        {
        }

        public void Fatal(Exception ex, string format, params object[] args)
        {
        }

        public void Info(string format, params object[] args)
        {
        }

        public void Trace(string format, params object[] args)
        {
        }

        public void Warn(string format, params object[] args)
        {
        }

        public void Warn(Exception ex, string format, params object[] args)
        {
        }
    }
}
