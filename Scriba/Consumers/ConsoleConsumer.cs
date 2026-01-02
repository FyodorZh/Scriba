using System;
using System.IO;

namespace Scriba.Consumers
{
    public class ConsoleConsumer : MultiRefLogConsumer
    {
        public ILogFormatter Formatter { get; set; } = new SynchronizedLogFormatter(DefaultFormatter);
        
        public override void Message(MessageData logMessage)
        {
            Formatter.Format(logMessage, Console.Out);
        }
        
        private static void DefaultFormatter(MessageData logMessage, TextWriter dst)
        {
            dst.Write(logMessage.Severity);
            dst.Write(": ");
            logMessage.WriteMessageTo(dst);
            logMessage.WriteStackTrace("\t", dst);
            dst.WriteLine();
        }
    }
}
