using System;

namespace Kontur.Rabbitmq.Tests
{
    internal class ConsoleLogService : ILogService
    {
        private readonly Type loggerType;

        public ConsoleLogService(Type loggerType)
        {
            this.loggerType = loggerType;
        }

        public void Trace(string format, params object[] args)
        {
            Console.Write($"{this.loggerType} ");
            Console.Write("[TRACE] ");
            Console.WriteLine(format, args);
        }

        public void Debug(string format, params object[] args)
        {
            Console.Write($"{this.loggerType} ");
            Console.Write("[DEBUG] ");
            Console.WriteLine(format, args);
        }

        public void Info(string format, params object[] args)
        {
            Console.Write($"{this.loggerType} ");
            Console.Write("[INFO] ");
            Console.WriteLine(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Console.Write($"{this.loggerType} ");
            Console.Write("[WARN] ");
            Console.WriteLine(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Console.Write($"{this.loggerType} ");
            Console.Write("[ERROR] ");
            Console.WriteLine(format, args);
        }

        public void Fatal(string format, params object[] args)
        {
            Console.Write($"{this.loggerType} ");
            Console.Write("[FATAL] ");
            Console.WriteLine(format, args);
        }

        public void Warn(Exception ex, string format, params object[] args)
        {
            Console.Write($"{this.loggerType} ");
            Console.Write("[WARN] ");
            Console.Write(format, args);
            Console.WriteLine(ex.ToString());
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            Console.Write($"{this.loggerType} ");
            Console.Write("[ERROR] ");
            Console.Write(format, args);
            Console.WriteLine(ex.ToString());
        }

        public void Fatal(Exception ex, string format, params object[] args)
        {
            Console.Write($"{this.loggerType} ");
            Console.Write("[FATAL] ");
            Console.Write(format, args);
            Console.WriteLine(ex.ToString());
        }
    }
}
