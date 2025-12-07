using System;

namespace Scriba.Consumers
{
    public class ConsoleConsumer : ILogConsumer
    {
        public static readonly ConsoleConsumer Instance = new ConsoleConsumer();
        
        private readonly System.IO.StringWriter _buffer = new ();

        public void Message(MessageData logMessage)
        {
            lock (_buffer)
            {
                _buffer.Write(logMessage.Severity);
                _buffer.Write(": ");
                logMessage.WriteMessageTo(_buffer);
                logMessage.WriteStackTrace("\t", _buffer);

                Console.WriteLine(_buffer);
                _buffer.GetStringBuilder().Length = 0;
            }
        }

        void ILogConsumer.AddRef()
        {
            // DO NOTHING
        }

        void ILogConsumer.Release()
        {
            // DO NOTHING
        }
    }
}
