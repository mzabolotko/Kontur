using System;

namespace Kontur.Rabbitmq.Tests
{
    internal class ConsoleLogService : ILogService
    {
        public void Trace(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Debug(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Info(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Fatal(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
