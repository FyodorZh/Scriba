using System;
using System.IO;

namespace Scriba
{
    public class SynchronizedLogFormatter : ILogFormatter
    {
        private readonly Action<MessageData, TextWriter> _formatter;
        private readonly StringWriter _sw = new StringWriter();

        public SynchronizedLogFormatter(Action<MessageData, TextWriter> formatter)
        {
            _formatter = formatter;
        }
        
        void ILogFormatter.Format(MessageData logMessage, TextWriter dst)
        {
            lock (_sw)
            {
                _sw.GetStringBuilder().Clear();
                _formatter(logMessage, _sw);
                dst.Write(_sw.ToString());
            }
        }
    }
}