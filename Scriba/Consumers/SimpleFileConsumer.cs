using System.IO;
using Scriba.JsonFactory;

namespace Scriba
{
    public class SimpleFileConsumer : MultiRefLogConsumer
    {
        private readonly StreamWriter _file;
        
        public ILogFormatter Formatter { get; set; } = new SynchronizedLogFormatter(DefaultFormatter);
        
        public SimpleFileConsumer(string fileName)
        {
            _file = File.AppendText(fileName);
        }
        
        public override void Message(MessageData logMessage)
        {
            Formatter.Format(logMessage, _file);
            _file.Flush();
        }

        protected override void Dispose()
        {
            _file.Dispose();
            base.Dispose();
        }
        
        private static void DefaultFormatter(MessageData logMessage, TextWriter dst)
        {
            logMessage.Data.Serialize(dst);
        }
    }
}