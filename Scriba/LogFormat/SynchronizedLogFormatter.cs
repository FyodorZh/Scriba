using System;
using System.IO;

namespace Scriba
{
    public class SynchronizedLogFormatter : ILogFormatter
    {
        private readonly Action<MessageData, TextWriter> _formatter;
        private readonly StringWriter _sw = new ();
        
        private readonly char[] _buffer = new char[1024];

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
                
                var sb = _sw.GetStringBuilder();
                int length = sb.Length;
                int offset = 0;
                while (length > 0)
                {
                    int chunk = Math.Min(length, _buffer.Length);
                    sb.CopyTo(offset, _buffer, 0, chunk);
                    dst.Write(_buffer, 0, chunk);
                    offset += chunk;
                    length -= chunk;
                }
            }
        }
    }
}