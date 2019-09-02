using NUnit.Framework;
using System;

namespace Kontur.Tests
{
    class NUnitLogService : ILogService
    {
        private readonly Type type;

        public NUnitLogService(Type type)
        {
            this.type = type;
        }

        public void Debug(string format, params object[] args)
        {
            this.Log("DEBUG", format, args);
        }

        public void Error(string format, params object[] args)
        {
            this.Log("ERROR", format, args);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            this.Log("ERROR", format, args);
        }

        public void Fatal(string format, params object[] args)
        {
            this.Log("FATAL", format, args);
        }

        public void Fatal(Exception ex, string format, params object[] args)
        {
            this.Log("FATAL", format, args);
        }

        public void Info(string format, params object[] args)
        {
            this.Log("INFO", format, args);
        }

        public void Trace(string format, params object[] args)
        {
            this.Log("TRACE", format, args);
        }

        public void Warn(string format, params object[] args)
        {
            this.Log("WARN", format, args);
        }

        public void Warn(Exception ex, string format, params object[] args)
        {
            this.Log("WARN", format, args);
        }

        private void Log(string level, string format, params object[] args)
        {
            string message = string.Format(format, args);
            string testName = TestContext.CurrentContext.Test.ClassName;
            string methodName = TestContext.CurrentContext.Test.Name;
            TestContext.Progress.Write($"[{methodName}] ({DateTime.Now.ToString("HH:mm:ss.fff")}) [{level.PadRight(5, ' ')}] {this.type.ToString().PadRight(15, ' ')} - {message}");
        }


    }
}
